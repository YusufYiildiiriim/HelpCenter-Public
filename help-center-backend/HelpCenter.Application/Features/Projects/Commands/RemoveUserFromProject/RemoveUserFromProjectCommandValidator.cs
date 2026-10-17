using FluentValidation;

namespace HelpCenter.Application.Features.Projects.Commands.RemoveUserFromProject;

public class RemoveUserFromProjectCommandValidator : AbstractValidator<RemoveUserFromProjectCommand>
{
    public RemoveUserFromProjectCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçersiz atama ID.");
    }
}
