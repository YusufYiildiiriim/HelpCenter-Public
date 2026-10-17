using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.DeleteFaq;

public record DeleteFaqCommand(Guid Id) : IRequest<bool>;
