using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.UpdateModule;

public class UpdateModuleCommand : IRequest<bool>
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public byte[]? RowVersion { get; set; }
    public List<int>? ExpertUserIds { get; set; }
}
