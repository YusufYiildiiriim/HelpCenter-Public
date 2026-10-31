using FluentValidation;

namespace HelpCenter.Application.Features.Guides.Commands.UpdateGuide;

public class UpdateGuideCommandValidator : AbstractValidator<UpdateGuideCommand>
{
    public UpdateGuideCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçersiz rehber ID.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık alanı boş olamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama alanı boş olamaz.");
        RuleFor(x => x.Module).NotEmpty().When(x => !x.ModuleId.HasValue).WithMessage("Modül (Kategori) seçimi zorunludur.");
        RuleFor(x => x.SourceLocale).Must(locale => locale is null || HelpCenter.Domain.Common.SupportedLocales.IsSupported(locale)).WithMessage("Desteklenmeyen locale.");
        RuleFor(x => x.Translations).Must(HelpCenter.Application.Common.Models.LocalizedTextRules.HasValidLocales).WithMessage("Çevirilerde locale hatalı veya tekrar ediyor.");
        RuleFor(x => x).Must(command => (command.Translations?.Count ?? 0) == 0 && (command.EnglishFiles?.Count ?? 0) == 0 || HelpCenter.Domain.Common.SupportedLocales.IsSupported(command.SourceLocale)).WithMessage("Çeviri girerken kaynak locale seçilmelidir.");
        RuleFor(x => x).Must(command =>
            (command.Translations?.Any(t => HelpCenter.Domain.Common.SupportedLocales.Normalize(t.Locale) == HelpCenter.Domain.Common.SupportedLocales.English && !string.IsNullOrWhiteSpace(t.Title) && !string.IsNullOrWhiteSpace(t.Description)) == true)
            || HelpCenter.Domain.Common.SupportedLocales.Normalize(command.SourceLocale) == HelpCenter.Domain.Common.SupportedLocales.English)
            .When(command => command.EnglishFiles?.Count > 0)
            .WithMessage("İngilizce dosyalar için İngilizce başlık ve içerik gerekir.");
        RuleFor(x => x.PreviousGuideId)
            .NotEmpty().WithMessage("Geçersiz önceki rehber ID.")
            .When(x => x.PreviousGuideId.HasValue);
    }
}
