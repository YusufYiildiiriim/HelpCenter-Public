using FluentValidation;

namespace HelpCenter.Application.Features.Requests.Commands.ConsultExpert;

public class ConsultExpertCommandValidator : AbstractValidator<ConsultExpertCommand>
{
    public ConsultExpertCommandValidator()
    {
        RuleFor(x => x.RequestPublicId).NotEmpty().WithMessage("Geçersiz talep.");
        RuleFor(x => x.ExpertId).GreaterThan(0).WithMessage("Geçersiz uzman ID.");
        RuleFor(x => x.AgentUserId).GreaterThan(0).WithMessage("Geçersiz temsilci ID.");
        RuleFor(x => x)
            .Must(x => x.ExpertId != x.AgentUserId)
            .WithMessage("Bir talebi kendinize yönlendiremezsiniz.")
            .WithName("ExpertId");
    }
}
