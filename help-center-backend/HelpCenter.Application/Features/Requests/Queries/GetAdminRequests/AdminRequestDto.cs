using HelpCenter.Domain.Enums;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;

public class AdminRequestDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string TicketId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public int? ModuleId { get; set; }
    public int DocumentCount { get; set; }
    public int MessageCount { get; set; }
    public RequestPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public string AssignedUserName { get; set; } = string.Empty;
    public string CurrentExpertName { get; set; } = string.Empty;
    public string RequestSubjectName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
