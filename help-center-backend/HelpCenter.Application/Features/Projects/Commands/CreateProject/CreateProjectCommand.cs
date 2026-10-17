using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommand : IRequest<bool>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<Guid>? UserIds { get; set; }
    public List<Guid>? ModuleIds { get; set; }
}
