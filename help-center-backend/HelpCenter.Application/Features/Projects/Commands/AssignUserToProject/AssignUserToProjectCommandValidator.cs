using FluentValidation;

namespace HelpCenter.Application.Features.Projects.Commands.AssignUserToProject;

public class AssignUserToProjectCommandValidator : AbstractValidator<AssignUserToProjectCommand>
{
    public AssignUserToProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("Geçersiz proje ID.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Geçersiz kullanıcı ID.");
    }
}
