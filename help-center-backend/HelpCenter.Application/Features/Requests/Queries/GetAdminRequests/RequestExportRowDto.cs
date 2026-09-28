namespace HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;

public class RequestExportRowDto
{
    public string TicketId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string PriorityName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssignedUserName { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
