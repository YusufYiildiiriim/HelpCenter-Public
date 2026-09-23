using FluentValidation;

namespace HelpCenter.Application.Features.Faqs.Commands.DeleteFaq;

public class DeleteFaqCommandValidator : AbstractValidator<DeleteFaqCommand>
{
    public DeleteFaqCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz SSS ID.");
    }
}
