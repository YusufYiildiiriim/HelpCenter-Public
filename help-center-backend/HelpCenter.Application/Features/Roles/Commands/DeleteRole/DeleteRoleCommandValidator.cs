using FluentValidation;

namespace HelpCenter.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçersiz rol ID.");
    }
}
