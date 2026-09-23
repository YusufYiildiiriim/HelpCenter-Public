using HelpCenter.Application.Common.Models;
using HelpCenter.Domain.Enums;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;

public class GetAdminRequestsQuery : IRequest<PaginatedResponse<AdminRequestDto>>
{
    public Guid? CompanyPublicId { get; set; }
    public int? CustomerId { get; set; }
    public string? Status { get; set; }
    public RequestPriority? Priority { get; set; }
    public int? AssignedUserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}
