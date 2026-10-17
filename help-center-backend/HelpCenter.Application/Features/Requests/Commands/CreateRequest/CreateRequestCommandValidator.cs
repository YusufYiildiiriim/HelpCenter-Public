using FluentValidation;

namespace HelpCenter.Application.Features.Requests.Commands.CreateRequest;

public class CreateRequestCommandValidator : AbstractValidator<CreateRequestCommand>
{
    public CreateRequestCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => !x.RequestSubjectId.HasValue)
            .WithMessage("Talep başlığı veya talep konusu belirtilmelidir.");
        RuleFor(x => x.MessageText).NotEmpty().WithMessage("Açıklama boş bırakılamaz.");
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Geçersiz kullanıcı.");
        RuleFor(x => x.Priority).IsInEnum().WithMessage("Lütfen geçerli bir öncelik seçiniz.");
        RuleFor(x => x.ModuleId)
            .NotEmpty().WithMessage("Geçersiz modül ID.")
            .When(x => x.ModuleId.HasValue);
        RuleFor(x => x.RequestSubjectId)
            .NotEmpty().WithMessage("Geçersiz talep konusu ID.")
            .When(x => x.RequestSubjectId.HasValue);
    }
}
