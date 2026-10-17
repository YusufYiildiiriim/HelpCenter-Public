using MediatR;

namespace HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;

public record GetUserPermissionsQuery(Guid UserId) : IRequest<UserPermissionsDto>;
