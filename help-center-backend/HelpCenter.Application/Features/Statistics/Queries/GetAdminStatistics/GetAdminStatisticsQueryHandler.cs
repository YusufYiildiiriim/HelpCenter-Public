using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Statistics.Queries.GetAdminStatistics;

public class GetAdminStatisticsQueryHandler : IRequestHandler<GetAdminStatisticsQuery, AdminStatisticsDto>
{
    private readonly IStatisticsReadRepository _statistics;
    private readonly ILogger<GetAdminStatisticsQueryHandler> _logger;
    private readonly IDataScopeService _scope;

    public GetAdminStatisticsQueryHandler(
        IStatisticsReadRepository statistics,
        ILogger<GetAdminStatisticsQueryHandler> logger,
        IDataScopeService scope)
    {
        _statistics = statistics;
        _logger = logger;
        _scope = scope;
    }

    public async Task<AdminStatisticsDto> Handle(GetAdminStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin istatistikleri getiriliyor.");

        var canRead = await _scope.HasPermissionAsync(AppResources.Dashboard, "Read", cancellationToken);
        GetAdminStatisticsRules.DashboardReadShouldBeAllowed(canRead);

        var allowedFieldSet = await _scope.GetAllowedFieldsAsync(AppResources.Dashboard, cancellationToken);
        var allowedFields = allowedFieldSet?.ToList();

        bool IsFieldAllowed(string fieldName) => allowedFields == null || allowedFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);

        var dto = new AdminStatisticsDto { AllowedFields = allowedFields };

        var row = await _statistics.GetDashboardStatsAsync(cancellationToken);

        foreach (var (column, value) in row)
        {
            if (IsFieldAllowed(column))
                dto.SetMetric(column, value);
        }

        _logger.LogInformation("Admin istatistikleri başarıyla getirildi (Dönüştürülmüş Harita Yapısı). İzinli Alan Sayısı: {AllowedFieldsCount}", allowedFields?.Count ?? -1);

        return dto;
    }
}
