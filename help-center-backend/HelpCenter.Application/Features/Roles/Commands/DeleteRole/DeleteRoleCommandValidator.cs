using FluentValidation;

namespace HelpCenter.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz rol ID.");
    }
}
