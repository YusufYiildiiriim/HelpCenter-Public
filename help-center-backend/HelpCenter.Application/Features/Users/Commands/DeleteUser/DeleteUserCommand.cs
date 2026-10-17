using HelpCenter.Application.Common.Caching;
using MediatR;

namespace HelpCenter.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<bool>, IInvalidatesMessageCache;
