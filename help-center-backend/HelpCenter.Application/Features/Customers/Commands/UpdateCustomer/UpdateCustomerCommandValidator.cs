using HelpCenter.Application.Common.Validation;
using FluentValidation;

namespace HelpCenter.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Geçersiz müşteri.");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Ad boş bırakılamaz.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Soyad boş bırakılamaz.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı boş bırakılamaz.")
            .ValidEmail().WithMessage("Geçerli bir e-posta giriniz.");
        RuleFor(x => x.CompanyId).NotEmpty().WithMessage("Geçersiz firma.");
    }
}
