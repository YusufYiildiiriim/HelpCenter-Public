using FluentValidation;

namespace HelpCenter.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz rol ID.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Rol adı boş bırakılamaz.");
    }
}
