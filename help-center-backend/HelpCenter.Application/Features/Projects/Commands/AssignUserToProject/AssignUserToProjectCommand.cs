using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.AssignUserToProject;

public class AssignUserToProjectCommand : IRequest<bool>
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }
}
