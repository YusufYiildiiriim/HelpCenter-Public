using FluentValidation;

namespace HelpCenter.Application.Features.Requests.Commands.CloseRequest;

public class CloseRequestCommandValidator : AbstractValidator<CloseRequestCommand>
{
    public CloseRequestCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Geçersiz talep.");
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Geçersiz müşteri.");
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Değerlendirme puanı 1 ile 5 arasında olmalıdır.");
    }
}
