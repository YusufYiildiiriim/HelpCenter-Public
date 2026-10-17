using FluentValidation;

namespace HelpCenter.Application.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Geçersiz proje ID.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Proje adı boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Proje açıklaması boş bırakılamaz.");
        RuleForEach(x => x.UserIds)
            .NotEmpty().WithMessage("Geçersiz kullanıcı ID.")
            .When(x => x.UserIds != null);
        RuleForEach(x => x.ModuleIds)
            .NotEmpty().WithMessage("Geçersiz modül ID.")
            .When(x => x.ModuleIds != null);
    }
}
