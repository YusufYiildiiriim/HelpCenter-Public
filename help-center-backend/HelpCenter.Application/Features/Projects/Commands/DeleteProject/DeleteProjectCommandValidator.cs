using FluentValidation;

namespace HelpCenter.Application.Features.Projects.Commands.DeleteProject;

public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Geçersiz proje ID.");
    }
}
