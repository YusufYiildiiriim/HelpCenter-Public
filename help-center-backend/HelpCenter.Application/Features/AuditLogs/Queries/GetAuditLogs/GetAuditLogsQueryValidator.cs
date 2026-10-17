using FluentValidation;

namespace HelpCenter.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.PageNumber).NotEmpty();
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.EventType).MaximumLength(100);
        RuleFor(x => x.TargetType).MaximumLength(100);
        RuleFor(x => x.To).Must((query, to) => !to.HasValue || !query.From.HasValue || to >= query.From);
    }
}
