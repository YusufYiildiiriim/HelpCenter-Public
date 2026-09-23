using FluentValidation;

namespace HelpCenter.Application.Features.Modules.Commands.DeleteModule;

public class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Geçersiz modül ID.");
    }
}
