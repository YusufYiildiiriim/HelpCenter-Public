using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.AssignUserToProject;

public class AssignUserToProjectCommand : IRequest<bool>
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
}
