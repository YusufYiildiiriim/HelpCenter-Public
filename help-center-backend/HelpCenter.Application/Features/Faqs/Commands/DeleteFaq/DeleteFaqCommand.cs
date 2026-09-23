using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.DeleteFaq;

public record DeleteFaqCommand(int Id) : IRequest<bool>;
