using HelpCenter.Application.Features.Statistics.Queries.GetAdminReports;

namespace HelpCenter.Application.Interfaces;

/// <summary>
/// Provider-neutral dashboard read model. Persistence owns query execution and returns
/// typed results so Application handlers do not depend on a database provider.
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

    Task<TicketFlowDto> GetTicketFlowAsync(CancellationToken cancellationToken = default);
    Task<List<DailyOpenedCountDto>> GetWeeklyOpenedAsync(int days, CancellationToken cancellationToken = default);
    Task<List<PriorityCountDto>> GetPriorityDistributionAsync(CancellationToken cancellationToken = default);
    Task<List<DashboardRecentRequestDto>> GetRecentRequestsAsync(int take, CancellationToken cancellationToken = default);
}
