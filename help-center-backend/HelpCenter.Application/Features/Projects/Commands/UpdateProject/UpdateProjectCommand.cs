using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommand : IRequest<bool>
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid RowVersion { get; set; }
    public List<Guid>? UserIds { get; set; }
    public List<Guid>? ModuleIds { get; set; }
}
