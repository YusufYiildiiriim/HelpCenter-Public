using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Statuses.Queries.GetAllStatuses;

public class GetAllStatusesQuery : IRequest<PaginatedResponse<StatusDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}
