using FluentValidation;

namespace HelpCenter.Application.Features.Auth.Commands.VerifyResetCode;

public class VerifyResetCodeCommandValidator : AbstractValidator<VerifyResetCodeCommand>
{
    public VerifyResetCodeCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername).NotEmpty().WithMessage("E-posta adresi veya kullanıcı adı boş olamaz.");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Doğrulama kodu boş olamaz.")
            .Length(6).WithMessage("Doğrulama kodu 6 haneli olmalıdır.");
    }
}
