using FluentValidation;

namespace HelpCenter.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz kullanıcı ID.");
    }
}
