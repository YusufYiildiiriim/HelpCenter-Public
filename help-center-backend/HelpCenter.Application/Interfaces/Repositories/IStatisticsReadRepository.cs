using HelpCenter.Application.Features.Statistics.Queries.GetAdminReports;

namespace HelpCenter.Application.Interfaces;

/// <summary>
/// Dashboard read model. Which stored procedure is called, the SQL dialect, and
/// the reader transformation are the responsibility of the Persistence layer; Application
/// only receives typed results. This way handlers do not carry raw SQL strings.
/// </summary>
public interface IStatisticsReadRepository
{
    /// <summary>Dashboard summary metrics (column name → value).</summary>
    Task<IReadOnlyDictionary<string, int>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);

    Task<List<NameCountDto>> GetStatusDistributionAsync(CancellationToken cancellationToken = default);

    Task<List<NameCountDto>> GetModuleDistributionAsync(CancellationToken cancellationToken = default);

    Task<List<NameCountDto>> GetCompanyTopNAsync(CancellationToken cancellationToken = default);

    Task<List<DailyTrendPointDto>> GetDailyTrendAsync(CancellationToken cancellationToken = default);

    Task<long> GetAvgResolutionMinutesAsync(CancellationToken cancellationToken = default);

    Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync(CancellationToken cancellationToken = default);

    Task<ReopenStatsDto> GetReopenStatsAsync(CancellationToken cancellationToken = default);
}
