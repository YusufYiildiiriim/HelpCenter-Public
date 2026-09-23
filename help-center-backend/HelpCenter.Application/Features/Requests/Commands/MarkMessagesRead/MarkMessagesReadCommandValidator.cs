using FluentValidation;

namespace HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;

public class MarkMessagesReadCommandValidator : AbstractValidator<MarkMessagesReadCommand>
{
    public MarkMessagesReadCommandValidator()
    {
        RuleFor(x => x.RequestPublicId).NotEmpty().WithMessage("Geçersiz talep.");
    }
}
