using FluentValidation;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.CreateRequestSubject;

public class CreateRequestSubjectCommandValidator : AbstractValidator<CreateRequestSubjectCommand>
{
    public CreateRequestSubjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Konu adı boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Konu açıklaması boş bırakılamaz.");
    }
}
