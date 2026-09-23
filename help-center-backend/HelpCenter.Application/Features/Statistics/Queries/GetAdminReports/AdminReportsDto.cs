namespace HelpCenter.Application.Features.Statistics.Queries.GetAdminReports;

public class NameCountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DailyTrendPointDto
{
    public DateTime Date { get; set; }
    public int Opened { get; set; }
    public int Closed { get; set; }
}

public class AgentPerformanceDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Assigned { get; set; }
    public int Completed { get; set; }
    public long AvgResolutionMinutes { get; set; }
}

public class ReopenStatsDto
{
    public int TotalCompleted { get; set; }
    public int Reopened { get; set; }
    public int RatePercent { get; set; }
}

public class AdminReportsDto
{
    public List<NameCountDto>? StatusDistribution { get; set; }
    public List<NameCountDto>? ModuleDistribution { get; set; }
    public List<NameCountDto>? CompanyTopN { get; set; }
    public List<DailyTrendPointDto>? DailyTrend { get; set; }
    public long? AvgResolutionMinutes { get; set; }
    public List<AgentPerformanceDto>? AgentPerformance { get; set; }
    public ReopenStatsDto? ReopenStats { get; set; }
    public List<string>? AllowedFields { get; set; }
}
