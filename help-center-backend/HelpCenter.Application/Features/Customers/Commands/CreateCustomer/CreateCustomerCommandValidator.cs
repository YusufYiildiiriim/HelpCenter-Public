using FluentValidation;

namespace HelpCenter.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Ad boş bırakılamaz.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Soyad boş bırakılamaz.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta giriniz.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        RuleFor(x => x.CompanyId).GreaterThan(0).WithMessage("Geçersiz firma.");
    }
}
