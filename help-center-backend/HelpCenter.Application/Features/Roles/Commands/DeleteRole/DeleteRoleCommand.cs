using MediatR;

namespace HelpCenter.Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(int Id) : IRequest<bool>;
