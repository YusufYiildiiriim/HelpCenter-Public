using HelpCenter.Application.Common.Validation;
using FluentValidation;

namespace HelpCenter.Application.Features.Organization.Commands.UpdateOrganizationInfo;

public class UpdateOrganizationInfoCommandValidator : AbstractValidator<UpdateOrganizationInfoCommand>
{
    public UpdateOrganizationInfoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz kurum ID.");
        RuleFor(x => x.OrganizationName).NotEmpty().WithMessage("Kurum adı boş bırakılamaz.");
        RuleFor(x => x.Email).ValidEmail().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Geçerli bir e-posta adresi giriniz.");
    }
}
