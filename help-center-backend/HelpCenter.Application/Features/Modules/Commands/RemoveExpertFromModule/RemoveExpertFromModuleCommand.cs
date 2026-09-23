using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.RemoveExpertFromModule;

public record RemoveExpertFromModuleCommand(int ExpertId) : IRequest<bool>;
