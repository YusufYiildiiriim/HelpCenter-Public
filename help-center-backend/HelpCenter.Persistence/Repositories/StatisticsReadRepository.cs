using System.Data;
using System.Data.Common;
using HelpCenter.Application.Features.Statistics.Queries.GetAdminReports;
using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Repositories;

/// <summary>
/// SQL Server implementation of the dashboard read model. All stored procedure names
/// and reader conversions are centralized here; the Application layer sees typed results.
/// </summary>
public sealed class StatisticsReadRepository : IStatisticsReadRepository
{
    private readonly EfContext _efContext;

    public StatisticsReadRepository(EfContext efContext)
    {
        _efContext = efContext;
    }

    public async Task<IReadOnlyDictionary<string, int>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        await using var reader = await ExecuteProcedureAsync("Sp_DashboardStats", cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result[reader.GetName(i)] = reader.IsDBNull(i) ? 0 : Convert.ToInt32(reader.GetValue(i));
            }
        }

        return result;
    }

    public Task<List<NameCountDto>> GetStatusDistributionAsync(CancellationToken cancellationToken = default) =>
        ReadNameCountsAsync("Sp_StatusDistribution", "StatusId", cancellationToken);

    public Task<List<NameCountDto>> GetModuleDistributionAsync(CancellationToken cancellationToken = default) =>
        ReadNameCountsAsync("Sp_ModuleDistribution", "ModuleId", cancellationToken);

    public Task<List<NameCountDto>> GetCompanyTopNAsync(CancellationToken cancellationToken = default) =>
        ReadNameCountsAsync("Sp_CompanyTopN", "CompanyId", cancellationToken);

    public async Task<List<DailyTrendPointDto>> GetDailyTrendAsync(CancellationToken cancellationToken = default)
    {
        var points = new List<DailyTrendPointDto>();

        await using var reader = await ExecuteProcedureAsync("Sp_DailyTrend", cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            points.Add(new DailyTrendPointDto
            {
                Date = Convert.ToDateTime(reader["Date"]),
                Opened = ToInt(reader["Opened"]),
                Closed = ToInt(reader["Closed"])
            });
        }

        return points;
    }

    public async Task<long> GetAvgResolutionMinutesAsync(CancellationToken cancellationToken = default)
    {
        await using var reader = await ExecuteProcedureAsync("Sp_AvgResolutionMinutes", cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ToLong(reader["AvgMinutes"]) : 0;
    }

    public async Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync(CancellationToken cancellationToken = default)
    {
        var rows = new List<AgentPerformanceDto>();

        await using var reader = await ExecuteProcedureAsync("Sp_AgentPerformance", cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new AgentPerformanceDto
            {
                UserId = ToInt(reader["UserId"]),
                FullName = reader["FullName"] as string ?? string.Empty,
                Assigned = ToInt(reader["Assigned"]),
                Completed = ToInt(reader["Completed"]),
                AvgResolutionMinutes = ToLong(reader["AvgResolutionMinutes"])
            });
        }

        return rows;
    }

    public async Task<ReopenStatsDto> GetReopenStatsAsync(CancellationToken cancellationToken = default)
    {
        await using var reader = await ExecuteProcedureAsync("Sp_ReopenStats", cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new ReopenStatsDto();
        }

        return new ReopenStatsDto
        {
            TotalCompleted = ToInt(reader["TotalCompleted"]),
            Reopened = ToInt(reader["Reopened"]),
            RatePercent = ToInt(reader["RatePercent"])
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
            .GroupBy(x => x.CreatedAt.Date)
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

    private async Task<List<NameCountDto>> ReadNameCountsAsync(string procedureName, string idColumn, CancellationToken cancellationToken)
    {
        var rows = new List<NameCountDto>();

        await using var reader = await ExecuteProcedureAsync(procedureName, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new NameCountDto
            {
                Id = ToInt(reader[idColumn]),
                Name = reader["Name"] as string ?? string.Empty,
                Count = ToInt(reader["Count"])
            });
        }

        return rows;
    }

    /// <summary>
    /// Invokes the procedure with CommandType.StoredProcedure: the name is never concatenated
    /// as a string, so no SQL injection surface remains.
    /// </summary>
    private async Task<DbDataReader> ExecuteProcedureAsync(string procedureName, CancellationToken cancellationToken)
    {
        var connection = _efContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var command = connection.CreateCommand();
        command.CommandText = procedureName;
        command.CommandType = CommandType.StoredProcedure;

        // The connection is managed by EF; do not use CloseConnection, otherwise the
        // DbContext's shared connection closes when the reader closes.
        return await command.ExecuteReaderAsync(cancellationToken);
    }

    private static int ToInt(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);

    private static long ToLong(object? value) => value is null or DBNull ? 0 : Convert.ToInt64(value);
}
