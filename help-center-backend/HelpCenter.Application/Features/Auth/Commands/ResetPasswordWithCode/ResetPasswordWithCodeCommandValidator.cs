using FluentValidation;

namespace HelpCenter.Application.Features.Auth.Commands.ResetPasswordWithCode;

public class ResetPasswordWithCodeCommandValidator : AbstractValidator<ResetPasswordWithCodeCommand>
{
    public ResetPasswordWithCodeCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername).NotEmpty().WithMessage("E-posta adresi veya kullanıcı adı boş olamaz.");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Doğrulama kodu boş olamaz.")
            .Length(6).WithMessage("Doğrulama kodu 6 haneli olmalıdır.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre boş olamaz.")
            .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır.");
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Şifre tekrarı boş olamaz.")
            .Equal(x => x.NewPassword).WithMessage("Şifreler uyuşmuyor.");
    }
}
