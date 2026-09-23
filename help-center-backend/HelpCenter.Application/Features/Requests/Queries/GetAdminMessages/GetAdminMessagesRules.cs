using HelpCenter.Application.Exceptions;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminMessages;

/// <summary>
/// Rules belonging only to the admin message listing slice.
/// </summary>
public static class GetAdminMessagesRules
{
    /// <summary>
    /// Verifies that the request exists in the system. A separate `IsDeleted` check is not needed —
    /// the projection query already goes through the default (filtered) query, so a soft-deleted
    /// row already returns `null`.
    /// </summary>
    public static void RequestShouldExist(AdminMessagesTicketSummary? ticket)
    {
        if (ticket == null)
        {
            throw new RequestNotFoundException();
        }
    }
}
