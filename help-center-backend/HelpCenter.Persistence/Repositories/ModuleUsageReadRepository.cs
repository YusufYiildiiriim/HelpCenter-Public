using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Repositories;

public sealed class ModuleUsageReadRepository(EfContext context) : IModuleUsageReadRepository
{
    public async Task<IReadOnlyDictionary<Guid, int>> CountAsync(
        IReadOnlyCollection<ModuleUsageLookup> modules,
        bool onlyActive,
        CancellationToken cancellationToken = default)
    {
        if (modules.Count == 0)
            return new Dictionary<Guid, int>();

        var ids = modules.Select(module => module.Id).Distinct().ToArray();
        var names = modules.Select(module => module.Name).Distinct(StringComparer.Ordinal).ToArray();

        // PostgreSQL text equality keeps case and trailing-space variants distinct by default.
        var linkedCounts = await context.Guides.Where(g => g.ModuleId.HasValue && ids.Contains(g.ModuleId.Value) && (!onlyActive || g.IsActive))
            .GroupBy(g => g.ModuleId!.Value).Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count, cancellationToken);

        var guideGroups = await context.Guides
            .Where(guide => !guide.ModuleId.HasValue && names.Contains(guide.Module) && (!onlyActive || guide.IsActive))
            .GroupBy(guide => guide.Module)
            .Select(group => new { Name = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var guideCounts = guideGroups.ToDictionary(group => group.Name, group => group.Count, StringComparer.Ordinal);

        // Execute sequentially: both queries use the same scoped DbContext.
        var requestCounts = await context.CustomerRequests
            .Where(request => request.ModuleId.HasValue && ids.Contains(request.ModuleId.Value))
            .GroupBy(request => request.ModuleId!.Value)
            .Select(group => new { ModuleId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(group => group.ModuleId, group => group.Count, cancellationToken);

        return modules.ToDictionary(
            module => module.Id,
            module => linkedCounts.GetValueOrDefault(module.Id) + guideCounts.GetValueOrDefault(module.Name) + requestCounts.GetValueOrDefault(module.Id));
    }
}
