using MediatR;

namespace HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;

public record GetUserPermissionsQuery(int UserId) : IRequest<UserPermissionsDto>;
