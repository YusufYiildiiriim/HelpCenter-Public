using FluentValidation;

namespace HelpCenter.Application.Features.Modules.Commands.RemoveExpertFromModule;

public class RemoveExpertFromModuleCommandValidator : AbstractValidator<RemoveExpertFromModuleCommand>
{
    public RemoveExpertFromModuleCommandValidator()
    {
        RuleFor(x => x.ExpertId).NotEmpty().WithMessage("Geçersiz uzman ID.");
    }
}
