using FluentValidation;

namespace HelpCenter.Application.Features.Modules.Commands.AssignExpertToModule;

public class AssignExpertToModuleCommandValidator : AbstractValidator<AssignExpertToModuleCommand>
{
    public AssignExpertToModuleCommandValidator()
    {
        RuleFor(x => x.ModuleId).NotEmpty().WithMessage("Geçersiz modül ID.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Geçersiz kullanıcı ID.");
    }
}
