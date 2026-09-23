using HelpCenter.Domain.Enums;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerRequests;

public class CustomerRequestDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string TicketId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public RequestPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public string RequestSubjectName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
