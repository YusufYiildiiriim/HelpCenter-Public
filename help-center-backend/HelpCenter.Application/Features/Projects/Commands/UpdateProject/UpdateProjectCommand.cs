using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommand : IRequest<bool>
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public byte[]? RowVersion { get; set; }
    public List<int>? UserIds { get; set; }
    public List<int>? ModuleIds { get; set; }
}
