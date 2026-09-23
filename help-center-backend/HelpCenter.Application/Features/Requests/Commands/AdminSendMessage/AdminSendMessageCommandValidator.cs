using FluentValidation;

namespace HelpCenter.Application.Features.Requests.Commands.AdminSendMessage;

public class AdminSendMessageCommandValidator : AbstractValidator<AdminSendMessageCommand>
{
    public AdminSendMessageCommandValidator()
    {
        RuleFor(x => x.RequestPublicId).NotEmpty().WithMessage("Geçersiz talep.");
        // Text may be empty when files are attached (file-only message); the handler supports this.
        RuleFor(x => x.MessageText)
            .NotEmpty()
            .When(x => x.Files == null || x.Files.Count == 0)
            .WithMessage("Mesaj metni boş olamaz.");
        RuleFor(x => x.SenderUserId).GreaterThan(0).WithMessage("Geçersiz kullanıcı.");
        RuleFor(x => x.Type).IsInEnum().WithMessage("Lütfen geçerli bir mesaj türü seçiniz.");
    }
}
