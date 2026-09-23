using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.RemoveUserFromProject;

public record RemoveUserFromProjectCommand(int Id) : IRequest<bool>;
