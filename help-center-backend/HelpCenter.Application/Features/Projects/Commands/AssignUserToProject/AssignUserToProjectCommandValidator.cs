using FluentValidation;

namespace HelpCenter.Application.Features.Projects.Commands.AssignUserToProject;

public class AssignUserToProjectCommandValidator : AbstractValidator<AssignUserToProjectCommand>
{
    public AssignUserToProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0).WithMessage("Geçersiz proje ID.");
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Geçersiz kullanıcı ID.");
    }
}
