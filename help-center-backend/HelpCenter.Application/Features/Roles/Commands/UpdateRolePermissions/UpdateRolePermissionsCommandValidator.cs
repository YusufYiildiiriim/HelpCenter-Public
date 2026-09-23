using FluentValidation;

namespace HelpCenter.Application.Features.Roles.Commands.UpdateRolePermissions;

public class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThan(0).WithMessage("Geçersiz rol ID.");
        // An empty list is a valid request: it means removing all permissions from the role
        // (see UpdateRolePermissionDto). That's why NotNull is used instead of NotEmpty.
        RuleFor(x => x.Permissions).NotNull().WithMessage("İzin listesi gönderilmelidir.");
        RuleForEach(x => x.Permissions)
            .ChildRules(p => p.RuleFor(x => x.ResourceKey).NotEmpty().WithMessage("Kaynak anahtarı boş olamaz."));
    }
}
