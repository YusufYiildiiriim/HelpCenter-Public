using FluentValidation;

namespace HelpCenter.Application.Features.Projects.Commands.RemoveUserFromProject;

public class RemoveUserFromProjectCommandValidator : AbstractValidator<RemoveUserFromProjectCommand>
{
    public RemoveUserFromProjectCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz atama ID.");
    }
}
