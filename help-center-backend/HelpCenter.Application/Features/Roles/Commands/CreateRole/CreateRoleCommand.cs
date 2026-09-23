using HelpCenter.Application.Features.Roles.Queries;
using HelpCenter.Application.Features.Roles.Queries.GetRoles;
using MediatR;

namespace HelpCenter.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommand : IRequest<RoleDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
