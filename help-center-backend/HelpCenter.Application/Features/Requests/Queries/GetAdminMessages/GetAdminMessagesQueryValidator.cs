using FluentValidation;
namespace HelpCenter.Application.Features.Requests.Queries.GetAdminMessages;
public class GetAdminMessagesQueryValidator : AbstractValidator<GetAdminMessagesQuery>
{
    public GetAdminMessagesQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.BeforeId).GreaterThan(0).When(x => x.BeforeId.HasValue);
    }
}
