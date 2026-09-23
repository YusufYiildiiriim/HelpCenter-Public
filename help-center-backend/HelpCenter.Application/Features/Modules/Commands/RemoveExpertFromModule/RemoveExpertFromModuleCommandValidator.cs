using FluentValidation;

namespace HelpCenter.Application.Features.Modules.Commands.RemoveExpertFromModule;

public class RemoveExpertFromModuleCommandValidator : AbstractValidator<RemoveExpertFromModuleCommand>
{
    public RemoveExpertFromModuleCommandValidator()
    {
        RuleFor(x => x.ExpertId).GreaterThan(0).WithMessage("Geçersiz uzman ID.");
    }
}
