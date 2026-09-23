using FluentValidation;

namespace HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;

public class UpdateFaqCommandValidator : AbstractValidator<UpdateFaqCommand>
{
    public UpdateFaqCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz SSS ID.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama boş bırakılamaz.");
        RuleFor(x => x.ProjectId).GreaterThan(0).When(x => x.ProjectId.HasValue).WithMessage("Geçersiz proje ID.");
        RuleFor(x => x.ModuleId).GreaterThan(0).When(x => x.ModuleId.HasValue).WithMessage("Geçersiz modül ID.");
    }
}
