using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQuery : IRequest<PaginatedResponse<ProjectDto>>
{
    public bool OnlyActive { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}
