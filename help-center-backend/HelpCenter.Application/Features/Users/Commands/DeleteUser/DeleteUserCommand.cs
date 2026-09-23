using MediatR;

namespace HelpCenter.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(int Id) : IRequest<bool>;
