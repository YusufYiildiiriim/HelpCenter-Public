using MediatR;

namespace HelpCenter.Application.Features.Guides.Commands.DeleteGuide;

public record DeleteGuideCommand(int Id) : IRequest<bool>;
