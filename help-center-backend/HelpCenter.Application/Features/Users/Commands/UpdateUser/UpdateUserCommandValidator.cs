using FluentValidation;

namespace HelpCenter.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz kullanıcı ID.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş bırakılamaz.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Soyad boş bırakılamaz.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı boş bırakılamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta giriniz.");
        RuleForEach(x => x.RoleIds)
            .GreaterThan(0).WithMessage("Geçersiz rol ID.")
            .When(x => x.RoleIds != null);
    }
}
