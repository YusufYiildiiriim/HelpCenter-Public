using FluentValidation;

namespace HelpCenter.Application.Features.Customers.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Geçersiz müşteri.");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Ad boş bırakılamaz.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Soyad boş bırakılamaz.");
    }
}
