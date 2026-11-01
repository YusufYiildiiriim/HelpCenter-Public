using FluentValidation;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.CreateRequestSubject;

public class CreateRequestSubjectCommandValidator : AbstractValidator<CreateRequestSubjectCommand>
{
    public CreateRequestSubjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Konu adı boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Konu açıklaması boş bırakılamaz.");
        RuleFor(x => x.SourceLocale).Must(locale => locale is null || HelpCenter.Domain.Common.SupportedLocales.IsSupported(locale));
        RuleFor(x => x.Translations).Must(HelpCenter.Application.Common.Models.LocalizedNameRules.HasValidLocales);
        RuleFor(x => x).Must(command => command.Translations.Count == 0 ||
            (HelpCenter.Domain.Common.SupportedLocales.IsSupported(command.SourceLocale) && command.Translations.Any(t =>
                HelpCenter.Domain.Common.SupportedLocales.Normalize(t.Locale) == HelpCenter.Domain.Common.SupportedLocales.Normalize(command.SourceLocale))))
            .WithMessage("Çeviri girerken kaynak locale çevirisi zorunludur.");
    }
}
