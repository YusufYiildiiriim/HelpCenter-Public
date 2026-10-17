using HelpCenter.Application.Common.Validation;
using FluentValidation;

namespace HelpCenter.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçersiz kullanıcı ID.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş bırakılamaz.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Soyad boş bırakılamaz.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı boş bırakılamaz.")
            .ValidEmail().WithMessage("Geçerli bir e-posta giriniz.");
        RuleForEach(x => x.RoleIds)
            .NotEmpty().WithMessage("Geçersiz rol ID.")
            .When(x => x.RoleIds != null);
    }
}
