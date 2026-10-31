using FluentValidation;

namespace HelpCenter.Application.Features.Faqs.Commands.CreateFaq;

public class CreateFaqCommandValidator : AbstractValidator<CreateFaqCommand>
{
    public CreateFaqCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama boş bırakılamaz.");
        RuleFor(x => x.ProjectId).NotEmpty().When(x => x.ProjectId.HasValue).WithMessage("Geçersiz proje ID.");
        RuleFor(x => x.ModuleId).NotEmpty().When(x => x.ModuleId.HasValue).WithMessage("Geçersiz modül ID.");
        RuleFor(x => x.SourceLocale).Must(HelpCenter.Domain.Common.SupportedLocales.IsSupported).WithMessage("Kaynak locale zorunludur ve desteklenmelidir.");
        RuleFor(x => x.Translations).Must(HelpCenter.Application.Common.Models.LocalizedTextRules.HasValidLocales).WithMessage("Çevirilerde locale hatalı veya tekrar ediyor.");
    }
}
