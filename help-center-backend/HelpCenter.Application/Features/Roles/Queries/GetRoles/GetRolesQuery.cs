using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Roles.Queries.GetRoles;

public class GetRolesQuery : IRequest<PaginatedResponse<RoleDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}
