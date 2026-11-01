using FluentValidation;

namespace HelpCenter.Application.Features.Modules.Commands.CreateModule;

public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Modül adı boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Modül açıklaması boş bırakılamaz.");
        RuleFor(x => x.Translations).Must(HelpCenter.Application.Common.Models.LocalizedNameRules.HasValidLocales);
        RuleFor(x => x).Must(x => HelpCenter.Application.Common.Models.LocalizedNameRules.HasValidSourceLocale(x.Translations, x.SourceLocale)).WithMessage("Kaynak dil çevirisi zorunludur.");
        RuleForEach(x => x.ExpertUserIds)
            .NotEmpty().WithMessage("Geçersiz uzman kullanıcı ID.")
            .When(x => x.ExpertUserIds != null);
    }
}
