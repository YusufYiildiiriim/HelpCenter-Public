using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Repositories;

public sealed class ModuleUsageReadRepository(EfContext context) : IModuleUsageReadRepository
{
    public async Task<IReadOnlyDictionary<int, int>> CountAsync(
        IReadOnlyCollection<ModuleUsageLookup> modules,
        bool onlyActive,
        CancellationToken cancellationToken = default)
    {
        if (modules.Count == 0)
            return new Dictionary<int, int>();

        var ids = modules.Select(module => module.Id).Distinct().ToArray();
        var names = modules.Select(module => module.Name).Distinct(StringComparer.Ordinal).ToArray();

        // The former in-memory comparison was ordinal. Binary grouping and byte length
        // keep case variants and trailing spaces separate under a case-insensitive database.
        var guideGroups = await context.Guides
            .Where(guide => names.Contains(guide.Module) && (!onlyActive || guide.IsActive))
            .GroupBy(guide => new
            {
                Name = EF.Functions.Collate(guide.Module, "Latin1_General_100_BIN2"),
                ByteLength = EF.Functions.DataLength(guide.Module)
            })
            .Select(group => new { group.Key.Name, Count = group.Count() })
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
            module => guideCounts.GetValueOrDefault(module.Name) + requestCounts.GetValueOrDefault(module.Id));
    }
}
