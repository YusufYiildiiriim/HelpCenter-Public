using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Commands.ConsultExpert;

/// <summary>
/// Rules belonging only to the consult-expert slice.
/// </summary>
public static class ConsultExpertRules
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
    /// Verifies that the agent exists in the system.
    /// </summary>
    public static void AgentShouldExist(User? agent)
    {
        if (agent == null)
        {
            throw new AgentNotFoundException();
        }
    }

    /// <summary>
    /// Verifies that the expert user to be consulted exists in the system.
    /// </summary>
    public static void ExpertShouldExist(User? expert)
    {
        if (expert == null)
        {
            throw new ExpertNotFoundException();
        }
    }
}
