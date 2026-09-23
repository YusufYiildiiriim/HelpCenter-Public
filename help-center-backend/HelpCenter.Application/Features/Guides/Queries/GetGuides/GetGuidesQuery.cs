using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Queries.GetGuides;

public class GetGuidesQuery : IRequest<PaginatedResponse<GuideDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Module { get; set; }
}
