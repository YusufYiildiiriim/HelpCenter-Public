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
