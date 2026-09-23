using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Statistics.Queries.GetAdminReports;

public class GetAdminReportsQueryHandler : IRequestHandler<GetAdminReportsQuery, AdminReportsDto>
{
    private readonly IStatisticsReadRepository _statistics;
    private readonly ILogger<GetAdminReportsQueryHandler> _logger;
    private readonly IDataScopeService _scope;

    public GetAdminReportsQueryHandler(IStatisticsReadRepository statistics, ILogger<GetAdminReportsQueryHandler> logger, IDataScopeService scope)
    {
        _statistics = statistics;
        _logger = logger;
        _scope = scope;
    }

    public async Task<AdminReportsDto> Handle(GetAdminReportsQuery request, CancellationToken cancellationToken)
    {
        var canRead = await _scope.HasPermissionAsync(AppResources.Dashboard, "Read", cancellationToken);
        GetAdminReportsRules.ReportsReadShouldBeAllowed(canRead);

        var allowedFieldSet = await _scope.GetAllowedFieldsAsync(AppResources.Dashboard, cancellationToken);
        var allowedFields = allowedFieldSet?.ToList();
        bool Allow(string k) => allowedFields == null || allowedFields.Contains(k, StringComparer.OrdinalIgnoreCase);

        var dto = new AdminReportsDto { AllowedFields = allowedFields };

        if (Allow(DashboardWidgets.StatusDistribution))
            dto.StatusDistribution = await _statistics.GetStatusDistributionAsync(cancellationToken);

        if (Allow(DashboardWidgets.ModuleDistribution))
            dto.ModuleDistribution = await _statistics.GetModuleDistributionAsync(cancellationToken);

        if (Allow(DashboardWidgets.CompanyTopN))
            dto.CompanyTopN = await _statistics.GetCompanyTopNAsync(cancellationToken);

        if (Allow(DashboardWidgets.DailyTrend))
            dto.DailyTrend = await _statistics.GetDailyTrendAsync(cancellationToken);

        if (Allow(DashboardWidgets.AvgResolutionMinutes))
            dto.AvgResolutionMinutes = await _statistics.GetAvgResolutionMinutesAsync(cancellationToken);

        if (Allow(DashboardWidgets.AgentPerformance))
            dto.AgentPerformance = await _statistics.GetAgentPerformanceAsync(cancellationToken);

        if (Allow(DashboardWidgets.ReopenRate))
            dto.ReopenStats = await _statistics.GetReopenStatsAsync(cancellationToken);

        _logger.LogInformation("Admin reports getirildi. İzinli alan sayısı: {Count}", allowedFields?.Count ?? -1);
        return dto;
    }
}
