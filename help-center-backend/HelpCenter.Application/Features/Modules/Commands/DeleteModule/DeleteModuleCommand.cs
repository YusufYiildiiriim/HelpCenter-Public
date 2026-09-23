using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.DeleteModule;

public record DeleteModuleCommand(Guid PublicId) : IRequest<bool>;
