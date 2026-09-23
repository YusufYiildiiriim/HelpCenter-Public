using FluentValidation;

namespace HelpCenter.Application.Features.Modules.Commands.AssignExpertToModule;

public class AssignExpertToModuleCommandValidator : AbstractValidator<AssignExpertToModuleCommand>
{
    public AssignExpertToModuleCommandValidator()
    {
        RuleFor(x => x.ModuleId).GreaterThan(0).WithMessage("Geçersiz modül ID.");
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Geçersiz kullanıcı ID.");
    }
}
