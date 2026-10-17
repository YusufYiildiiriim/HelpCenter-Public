using FluentValidation;

namespace HelpCenter.Application.Features.Guides.Commands.DeleteGuide;

public class DeleteGuideCommandValidator : AbstractValidator<DeleteGuideCommand>
{
    public DeleteGuideCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçersiz rehber ID.");
    }
}
