using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetModules;

public class GetModulesQuery : IRequest<PaginatedResponse<ModuleDto>>
{
    public bool OnlyActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}
