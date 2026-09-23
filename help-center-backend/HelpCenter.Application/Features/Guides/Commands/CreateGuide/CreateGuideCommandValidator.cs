using FluentValidation;

namespace HelpCenter.Application.Features.Guides.Commands.CreateGuide;

public class CreateGuideCommandValidator : AbstractValidator<CreateGuideCommand>
{
    public CreateGuideCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık alanı boş olamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama alanı boş olamaz.");
        RuleFor(x => x.Module).NotEmpty().WithMessage("Modül (Kategori) seçimi zorunludur.");
    }
}
