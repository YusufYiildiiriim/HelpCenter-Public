using FluentValidation;

namespace HelpCenter.Application.Features.Customers.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Geçersiz müşteri.");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Ad boş bırakılamaz.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Soyad boş bırakılamaz.");
    }
}
