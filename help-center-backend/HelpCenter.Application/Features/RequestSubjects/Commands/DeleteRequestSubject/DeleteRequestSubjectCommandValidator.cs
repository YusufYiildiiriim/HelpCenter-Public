using FluentValidation;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.DeleteRequestSubject;

public class DeleteRequestSubjectCommandValidator : AbstractValidator<DeleteRequestSubjectCommand>
{
    public DeleteRequestSubjectCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçersiz talep konusu ID.");
    }
}
