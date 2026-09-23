using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;
using HelpCenter.Application.Common.Models;
using HelpCenter.Domain.Enums;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetAssignedRequests;

public class GetAssignedRequestsQuery : IRequest<PaginatedResponse<AdminRequestDto>>
{
    public string? Status { get; set; }
    public RequestPriority? Priority { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
