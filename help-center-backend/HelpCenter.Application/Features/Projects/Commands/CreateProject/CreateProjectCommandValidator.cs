using FluentValidation;

namespace HelpCenter.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Proje adı boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Proje açıklaması boş bırakılamaz.");
        RuleFor(x => x.Translations).Must(HelpCenter.Application.Common.Models.LocalizedNameRules.HasValidLocales);
        RuleFor(x => x).Must(x => HelpCenter.Application.Common.Models.LocalizedNameRules.HasValidSourceLocale(x.Translations, x.SourceLocale)).WithMessage("Kaynak dil çevirisi zorunludur.");
        RuleForEach(x => x.UserIds)
            .NotEmpty().WithMessage("Geçersiz kullanıcı ID.")
            .When(x => x.UserIds != null);
        RuleForEach(x => x.ModuleIds)
            .NotEmpty().WithMessage("Geçersiz modül ID.")
            .When(x => x.ModuleIds != null);
    }
}
