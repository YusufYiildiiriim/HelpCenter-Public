using FluentValidation;

namespace HelpCenter.Application.Features.Companies.Commands.DeleteCompany;

public class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
{
    public DeleteCompanyCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Geçersiz firma ID.");
    }
}
