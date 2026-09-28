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
    public DashboardOverviewDto? Dashboard { get; set; }
    public List<NameCountDto>? StatusDistribution { get; set; }
    public List<NameCountDto>? ModuleDistribution { get; set; }
    public List<NameCountDto>? CompanyTopN { get; set; }
    public List<DailyTrendPointDto>? DailyTrend { get; set; }
    public long? AvgResolutionMinutes { get; set; }
    public List<AgentPerformanceDto>? AgentPerformance { get; set; }
    public ReopenStatsDto? ReopenStats { get; set; }
    public List<string>? AllowedFields { get; set; }
}

public class DashboardOverviewDto
{
    public TicketFlowDto? TicketFlow { get; set; }
    public List<DailyOpenedCountDto>? WeeklyOpened { get; set; }
    public List<PriorityCountDto>? PriorityDistribution { get; set; }
    public List<DashboardRecentRequestDto>? RecentRequests { get; set; }
}

public class TicketFlowDto
{
    public int Total { get; set; }
    public List<NameCountDto> Stages { get; set; } = [];
}

public class DailyOpenedCountDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class PriorityCountDto
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardRecentRequestDto
{
    public Guid PublicId { get; set; }
    public string TicketId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PriorityName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
