using HelpCenter.Application.Features.Statistics.Queries.GetAdminReports;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Enums;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Repositories;

/// <summary>
/// Provider-neutral dashboard read model expressed with EF Core queries.
/// </summary>
public sealed class StatisticsReadRepository : IStatisticsReadRepository
{
    private static readonly Guid[] ActiveRequestStatusIds =
    [
        RequestStatusConstants.WaitingForReply,
        RequestStatusConstants.Replied,
        RequestStatusConstants.UnderTechnicalReview
    ];

    private readonly EfContext _efContext;

    public StatisticsReadRepository(EfContext efContext)
    {
        _efContext = efContext;
    }

    public async Task<IReadOnlyDictionary<string, int>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var requestStats = await _efContext.CustomerRequests
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                Active = group.Count(request => ActiveRequestStatusIds.Contains(request.StatusId)),
                Completed = group.Count(request => request.StatusId == RequestStatusConstants.Completed)
            })
            .SingleOrDefaultAsync(cancellationToken);

        var totalMessages = await _efContext.CustomerRequestMessages.AsNoTracking().CountAsync(cancellationToken);
        var totalCustomers = await _efContext.Customers.AsNoTracking()
            .CountAsync(customer => customer.IsActive, cancellationToken);
        var totalCompanies = await _efContext.Companies.AsNoTracking().CountAsync(cancellationToken);

        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["TotalRequests"] = requestStats?.Total ?? 0,
            ["ActiveRequests"] = requestStats?.Active ?? 0,
            ["CompletedRequests"] = requestStats?.Completed ?? 0,
            ["TotalMessages"] = totalMessages,
            ["TotalCustomers"] = totalCustomers,
            ["TotalCompanies"] = totalCompanies
        };
    }

    public Task<List<NameCountDto>> GetStatusDistributionAsync(CancellationToken cancellationToken = default) =>
        _efContext.CustomerRequestStatuses
            .AsNoTracking()
            .Select(status => new NameCountDto
            {
                Id = status.Id,
                Name = status.Name,
                Count = _efContext.CustomerRequests.Count(request => request.StatusId == status.Id)
            })
            .OrderByDescending(item => item.Count)
            .ToListAsync(cancellationToken);

    public Task<List<NameCountDto>> GetModuleDistributionAsync(CancellationToken cancellationToken = default) =>
        _efContext.Modules
            .AsNoTracking()
            .Select(module => new NameCountDto
            {
                Id = module.Id,
                Name = module.Name,
                Count = _efContext.CustomerRequests.Count(request => request.ModuleId == module.Id)
            })
            .OrderByDescending(item => item.Count)
            .ToListAsync(cancellationToken);

    public Task<List<NameCountDto>> GetCompanyTopNAsync(CancellationToken cancellationToken = default) =>
        _efContext.Companies
            .AsNoTracking()
            .Select(company => new NameCountDto
            {
                Id = company.Id,
                Name = company.Name,
                Count = _efContext.Customers
                    .Where(customer => customer.CompanyId == company.Id)
                    .SelectMany(customer => _efContext.CustomerRequests.Where(request => request.CustomerId == customer.Id))
                    .Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Name)
            .Take(10)
            .ToListAsync(cancellationToken);

    public async Task<List<DailyTrendPointDto>> GetDailyTrendAsync(CancellationToken cancellationToken = default)
    {
        const int days = 30;
        var today = DateTime.UtcNow.Date;
        var start = today.AddDays(-(days - 1));
        var end = today.AddDays(1);

        var openedRows = await _efContext.CustomerRequests
            .AsNoTracking()
            .Where(request => request.CreatedAt >= start && request.CreatedAt < end)
            .GroupBy(request => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(request.CreatedAt, "UTC").Date)
            .Select(group => new { Date = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var closedRows = await _efContext.CustomerRequests
            .AsNoTracking()
            .Where(request => request.StatusId == RequestStatusConstants.Completed
                && request.UpdatedAt.HasValue
                && request.UpdatedAt.Value >= start
                && request.UpdatedAt.Value < end)
            .GroupBy(request => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(request.UpdatedAt!.Value, "UTC").Date)
            .Select(group => new { Date = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var openedByDate = openedRows.ToDictionary(row => row.Date, row => row.Count);
        var closedByDate = closedRows.ToDictionary(row => row.Date, row => row.Count);

        return Enumerable.Range(0, days)
            .Select(offset => start.AddDays(offset))
            .Select(date => new DailyTrendPointDto
            {
                Date = date,
                Opened = openedByDate.GetValueOrDefault(date),
                Closed = closedByDate.GetValueOrDefault(date)
            })
            .ToList();
    }

    public async Task<long> GetAvgResolutionMinutesAsync(CancellationToken cancellationToken = default)
    {
        var averageMinutes = await _efContext.CustomerRequests
            .AsNoTracking()
            .Where(request => request.StatusId == RequestStatusConstants.Completed && request.UpdatedAt.HasValue)
            .Select(request => (double?)(
                Math.Floor((request.UpdatedAt!.Value - DateTime.UnixEpoch).TotalMinutes)
                - Math.Floor((request.CreatedAt - DateTime.UnixEpoch).TotalMinutes)))
            .AverageAsync(cancellationToken);

        return (long)(averageMinutes ?? 0);
    }

    public async Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync(CancellationToken cancellationToken = default)
    {
        return await _efContext.Users
            .AsNoTracking()
            .Where(user => user.IsActive && _efContext.CustomerRequests.Any(request => request.AssignedUserId == user.Id))
            .Select(user => new AgentPerformanceDto
            {
                UserId = user.Id,
                FullName = user.Account.FirstName + " " + user.Account.LastName,
                Assigned = _efContext.CustomerRequests.Count(request => request.AssignedUserId == user.Id),
                Completed = _efContext.CustomerRequests.Count(request => request.AssignedUserId == user.Id
                    && request.StatusId == RequestStatusConstants.Completed),
                AvgResolutionMinutes = (long)(_efContext.CustomerRequests
                    .Where(request => request.AssignedUserId == user.Id
                        && request.StatusId == RequestStatusConstants.Completed
                        && request.UpdatedAt.HasValue)
                    .Select(request => (double?)(
                        Math.Floor((request.UpdatedAt!.Value - DateTime.UnixEpoch).TotalMinutes)
                        - Math.Floor((request.CreatedAt - DateTime.UnixEpoch).TotalMinutes)))
                    .Average() ?? 0)
            })
            .OrderByDescending(agent => agent.Assigned)
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReopenStatsDto> GetReopenStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalCompleted = await _efContext.CustomerRequests
            .CountAsync(request => request.StatusId == RequestStatusConstants.Completed, cancellationToken);
        var reopened = await _efContext.RequestHistories
            .Where(history => history.Action == RequestActionType.StatusChanged
                && history.OldValue == RequestStatusConstants.Completed.ToString("D"))
            .Select(history => history.RequestId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new ReopenStatsDto
        {
            TotalCompleted = totalCompleted,
            Reopened = reopened,
            RatePercent = totalCompleted == 0 ? 0 : (int)(reopened * 100.0 / totalCompleted)
        };
    }

    public async Task<TicketFlowDto> GetTicketFlowAsync(CancellationToken cancellationToken = default)
    {
        var stages = await _efContext.CustomerRequests
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => new { x.StatusId, x.Status.Name })
            .Select(x => new NameCountDto { Id = x.Key.StatusId, Name = x.Key.Name, Count = x.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return new TicketFlowDto { Total = stages.Sum(x => x.Count), Stages = stages };
    }

    public async Task<List<DailyOpenedCountDto>> GetWeeklyOpenedAsync(int days, CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow.Date.AddDays(-(Math.Max(days, 1) - 1));
        var rows = await _efContext.CustomerRequests
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.CreatedAt >= start)
            .GroupBy(x => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(x.CreatedAt, "UTC").Date)
            .Select(x => new DailyOpenedCountDto { Date = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var counts = rows.ToDictionary(x => x.Date, x => x.Count);
        return Enumerable.Range(0, Math.Max(days, 1))
            .Select(offset => start.AddDays(offset))
            .Select(date => new DailyOpenedCountDto { Date = date, Count = counts.GetValueOrDefault(date) })
            .ToList();
    }

    public async Task<List<PriorityCountDto>> GetPriorityDistributionAsync(CancellationToken cancellationToken = default)
    {
        var grouped = await _efContext.CustomerRequests
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.Priority)
            .Select(x => new { Priority = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        return Enum.GetValues<HelpCenter.Domain.Enums.RequestPriority>()
            .Select(priority => new PriorityCountDto
            {
                Priority = priority.ToString(),
                Count = grouped.FirstOrDefault(x => x.Priority == priority)?.Count ?? 0
            })
            .ToList();
    }

    public Task<List<DashboardRecentRequestDto>> GetRecentRequestsAsync(int take, CancellationToken cancellationToken = default) =>
        _efContext.CustomerRequests
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(Math.Clamp(take, 1, 50))
            .Select(x => new DashboardRecentRequestDto
            {
                PublicId = x.PublicId,
                TicketId = x.TicketId,
                Title = x.Title,
                Status = x.Status.Name,
                PriorityName = x.Priority.ToString(),
                CustomerName = x.Customer.Account.FirstName + " " + x.Customer.Account.LastName,
                CompanyName = x.Customer.Company.Name,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

}
