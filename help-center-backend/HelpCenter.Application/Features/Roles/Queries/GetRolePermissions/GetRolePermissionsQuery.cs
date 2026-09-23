using MediatR;

namespace HelpCenter.Application.Features.Roles.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(int RoleId) : IRequest<List<RolePermissionDto>>;
