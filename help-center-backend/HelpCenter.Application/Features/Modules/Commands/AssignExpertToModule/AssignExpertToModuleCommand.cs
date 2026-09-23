using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.AssignExpertToModule;

public class AssignExpertToModuleCommand : IRequest<bool>
{
    public int ModuleId { get; set; }
    public int UserId { get; set; }
}
