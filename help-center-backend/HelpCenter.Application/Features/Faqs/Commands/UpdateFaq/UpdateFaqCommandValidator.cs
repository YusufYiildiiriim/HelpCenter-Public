using FluentValidation;

namespace HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;

public class UpdateFaqCommandValidator : AbstractValidator<UpdateFaqCommand>
{
    public UpdateFaqCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçersiz SSS ID.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama boş bırakılamaz.");
        RuleFor(x => x.SourceLocale).Must(locale => locale is null || HelpCenter.Domain.Common.SupportedLocales.IsSupported(locale)).WithMessage("Desteklenmeyen locale.");
        RuleFor(x => x.Translations).Must(HelpCenter.Application.Common.Models.LocalizedTextRules.HasValidLocales).WithMessage("Çevirilerde locale hatalı veya tekrar ediyor.");
RuleFor(x => x).Must(command => command.Translations is null || command.Translations.Count == 0 || HelpCenter.Domain.Common.SupportedLocales.IsSupported(command.SourceLocale)).WithMessage("Çeviri girerken kaynak locale seçilmelidir.");
        RuleFor(x => x.ProjectId).NotEmpty().When(x => x.ProjectId.HasValue).WithMessage("Geçersiz proje ID.");
        RuleFor(x => x.ModuleId).NotEmpty().When(x => x.ModuleId.HasValue).WithMessage("Geçersiz modül ID.");
    }
}
