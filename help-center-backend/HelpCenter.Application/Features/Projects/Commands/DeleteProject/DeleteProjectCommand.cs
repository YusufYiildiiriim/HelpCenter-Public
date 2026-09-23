using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid PublicId) : IRequest<bool>;
