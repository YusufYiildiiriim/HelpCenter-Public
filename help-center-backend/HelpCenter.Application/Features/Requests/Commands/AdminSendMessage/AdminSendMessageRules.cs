using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Commands.AdminSendMessage;

/// <summary>
/// Rules belonging only to the admin send-message slice.
/// </summary>
public static class AdminSendMessageRules
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
    /// Verifies that the request is not completed (messages cannot be sent to a completed request).
    /// </summary>
    public static void RequestShouldNotBeCompleted(CustomerRequest customerRequest)
    {
        if (customerRequest.StatusId == RequestStatusConstants.Completed)
        {
            throw new RequestCompletedException("Tamamlanmış bir talebe mesaj gönderilemez.");
        }
    }

    /// <summary>
    /// Verifies that the agent / admin user exists in the system.
    /// </summary>
    public static void AgentShouldExist(User? agent)
    {
        if (agent == null)
        {
            throw new AgentNotFoundException();
        }
    }
}
