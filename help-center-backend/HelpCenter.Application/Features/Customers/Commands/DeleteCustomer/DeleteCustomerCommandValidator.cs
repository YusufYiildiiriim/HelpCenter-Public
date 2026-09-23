using FluentValidation;

namespace HelpCenter.Application.Features.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Geçersiz müşteri ID.");
    }
}
