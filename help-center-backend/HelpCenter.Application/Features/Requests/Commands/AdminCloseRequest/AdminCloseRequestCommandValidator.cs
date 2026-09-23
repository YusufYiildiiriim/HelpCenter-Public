using FluentValidation;

namespace HelpCenter.Application.Features.Requests.Commands.AdminCloseRequest;

public class AdminCloseRequestCommandValidator : AbstractValidator<AdminCloseRequestCommand>
{
    public AdminCloseRequestCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz talep ID.");
    }
}
