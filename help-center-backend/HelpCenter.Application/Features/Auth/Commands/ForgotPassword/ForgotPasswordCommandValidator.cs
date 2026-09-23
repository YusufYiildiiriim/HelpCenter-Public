using FluentValidation;

namespace HelpCenter.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername).NotEmpty().WithMessage("E-posta adresi veya kullanıcı adı boş olamaz.");
    }
}
