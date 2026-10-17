using MediatR;

namespace HelpCenter.Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(Guid Id) : IRequest<bool>;
