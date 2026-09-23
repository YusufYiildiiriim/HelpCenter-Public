using FluentValidation;

namespace HelpCenter.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("E-posta adresi veya kullanıcı adı boş olamaz.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre boş olamaz.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Şifre tekrarı boş olamaz.")
            .Equal(x => x.NewPassword).WithMessage("Şifreler uyuşmuyor.");
    }
}
