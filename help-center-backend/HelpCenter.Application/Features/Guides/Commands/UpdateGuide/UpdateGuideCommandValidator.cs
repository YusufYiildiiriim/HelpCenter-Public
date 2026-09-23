using FluentValidation;

namespace HelpCenter.Application.Features.Guides.Commands.UpdateGuide;

public class UpdateGuideCommandValidator : AbstractValidator<UpdateGuideCommand>
{
    public UpdateGuideCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz rehber ID.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık alanı boş olamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama alanı boş olamaz.");
        RuleFor(x => x.Module).NotEmpty().WithMessage("Modül (Kategori) seçimi zorunludur.");
        RuleFor(x => x.PreviousGuideId)
            .GreaterThan(0).WithMessage("Geçersiz önceki rehber ID.")
            .When(x => x.PreviousGuideId.HasValue);
    }
}
