namespace HelpCenter.Application.Interfaces;

public sealed record ModuleUsageLookup(int Id, string Name);

/// <summary>Counts usage for the requested module page without materializing its related entities.</summary>
public interface IModuleUsageReadRepository
{
    Task<IReadOnlyDictionary<int, int>> CountAsync(
        IReadOnlyCollection<ModuleUsageLookup> modules,
        bool onlyActive,
        CancellationToken cancellationToken = default);
}
