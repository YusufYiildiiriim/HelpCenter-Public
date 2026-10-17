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
    }
}
