using MediatR;

namespace HelpCenter.Application.Features.Guides.Commands.DeleteGuide;

public record DeleteGuideCommand(Guid Id) : IRequest<bool>;
