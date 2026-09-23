using FluentValidation;

namespace HelpCenter.Application.Features.Requests.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.RequestPublicId).NotEmpty().WithMessage("Geçersiz talep.");
        // Text may be empty when files are attached (file-only message); the handler supports this.
        RuleFor(x => x.MessageText)
            .NotEmpty()
            .When(x => x.Files == null || x.Files.Count == 0)
            .WithMessage("Mesaj metni boş olamaz.");
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Geçersiz kullanıcı.");
    }
}
