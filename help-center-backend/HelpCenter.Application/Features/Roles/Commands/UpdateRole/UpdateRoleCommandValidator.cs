using FluentValidation;

namespace HelpCenter.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçersiz rol ID.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Rol adı boş bırakılamaz.");
    }
}
