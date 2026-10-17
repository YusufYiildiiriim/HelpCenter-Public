using FluentValidation;

namespace HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;

public class UpdateFaqCommandValidator : AbstractValidator<UpdateFaqCommand>
{
    public UpdateFaqCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçersiz SSS ID.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama boş bırakılamaz.");
        RuleFor(x => x.ProjectId).NotEmpty().When(x => x.ProjectId.HasValue).WithMessage("Geçersiz proje ID.");
        RuleFor(x => x.ModuleId).NotEmpty().When(x => x.ModuleId.HasValue).WithMessage("Geçersiz modül ID.");
    }
}
