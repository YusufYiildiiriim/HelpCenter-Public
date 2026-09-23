using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.CreateModule;

public class CreateModuleCommand : IRequest<bool>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<int>? ExpertUserIds { get; set; }
}
