using FluentValidation;

namespace HelpCenter.Application.Features.Auth.Queries.UserLogin;

public class UserLoginQueryValidator : AbstractValidator<UserLoginQuery>
{
    public UserLoginQueryValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("E-posta veya kullanıcı adı boş bırakılamaz");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre alanı boş bırakılamaz");
    }
}
