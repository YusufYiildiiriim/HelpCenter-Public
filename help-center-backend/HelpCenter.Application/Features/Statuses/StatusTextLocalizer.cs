using HelpCenter.Domain.Common;
using HelpCenter.Domain.Constants;

namespace HelpCenter.Application.Features.Statuses;

public static class StatusTextLocalizer
{
    public static (string Name, string Description) Localize(Guid statusId, string name, string description, string? locale)
    {
        if (SupportedLocales.Normalize(locale) != SupportedLocales.English)
            return (name, description);

        return statusId switch
        {
            var id when id == RequestStatusConstants.WaitingForReply =>
                ("Waiting for reply", "The request is waiting for a response from support."),
            var id when id == RequestStatusConstants.Replied =>
                ("Replied", "A support representative has responded to the request."),
            var id when id == RequestStatusConstants.Completed =>
                ("Completed", "The request has been resolved and closed."),
            var id when id == RequestStatusConstants.UnderTechnicalReview =>
                ("Under technical review", "An expert is reviewing the request."),
            _ => (name, description)
        };
    }
}
