using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Commands.AdminCloseRequest;

/// <summary>
/// Rules belonging only to the admin close-request slice.
/// </summary>
public static class AdminCloseRequestRules
{
    /// <summary>
    /// Verifies that the request exists in the system.
    /// </summary>
    public static void RequestShouldExist(CustomerRequest? customerRequest)
    {
        if (customerRequest == null || customerRequest.IsDeleted)
        {
            throw new RequestNotFoundException();
        }
    }

    /// <summary>
    /// Verifies that the authenticated actor still has a staff user record.
    /// </summary>
    public static void ActorShouldExist(User? actor)
    {
        if (actor == null || actor.IsDeleted)
        {
            throw new UserNotFoundException();
        }
    }
}
