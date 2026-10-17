using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.RemoveExpertFromModule;

public record RemoveExpertFromModuleCommand(Guid ExpertId) : IRequest<bool>;
