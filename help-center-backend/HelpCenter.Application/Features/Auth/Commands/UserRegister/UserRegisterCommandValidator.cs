using FluentValidation;

namespace HelpCenter.Application.Features.Auth.Commands.UserRegister;

public class UserRegisterCommandValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı boş bırakılamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad alanı boş bırakılamaz.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Soyad alanı boş bırakılamaz.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre alanı boş bırakılamaz.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        RuleForEach(x => x.RoleIds)
            .GreaterThan(0).WithMessage("Geçersiz rol ID.")
            .When(x => x.RoleIds != null);
    }
}
