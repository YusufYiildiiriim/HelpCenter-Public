using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.RemoveUserFromProject;

public record RemoveUserFromProjectCommand(Guid Id) : IRequest<bool>;
