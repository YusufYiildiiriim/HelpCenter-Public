using FluentValidation;

namespace HelpCenter.Application.Features.Requests.Commands.ConsultExpert;

public class ConsultExpertCommandValidator : AbstractValidator<ConsultExpertCommand>
{
    public ConsultExpertCommandValidator()
    {
        RuleFor(x => x.RequestPublicId).NotEmpty().WithMessage("Geçersiz talep.");
        RuleFor(x => x.ExpertId).NotEmpty().WithMessage("Geçersiz uzman ID.");
        RuleFor(x => x.AgentUserId).NotEmpty().WithMessage("Geçersiz temsilci ID.");
        RuleFor(x => x)
            .Must(x => x.ExpertId != x.AgentUserId)
            .WithMessage("Bir talebi kendinize yönlendiremezsiniz.")
            .WithName("ExpertId");
    }
}
