using FluentValidation;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.UpdateRequestSubject;

public class UpdateRequestSubjectCommandValidator : AbstractValidator<UpdateRequestSubjectCommand>
{
    public UpdateRequestSubjectCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçersiz talep konusu ID.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Konu adı boş bırakılamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama boş bırakılamaz.");
    }
}
