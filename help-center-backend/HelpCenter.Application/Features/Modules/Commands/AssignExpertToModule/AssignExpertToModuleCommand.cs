using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.AssignExpertToModule;

public class AssignExpertToModuleCommand : IRequest<bool>
{
    public Guid ModuleId { get; set; }
    public Guid UserId { get; set; }
}
