using MediatR;

namespace HelpCenter.Application.Features.Roles.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(Guid RoleId) : IRequest<List<RolePermissionDto>>;
