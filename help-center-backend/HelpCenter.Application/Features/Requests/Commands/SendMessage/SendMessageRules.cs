using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Commands.SendMessage;

/// <summary>
/// Rules belonging only to the customer send-message slice.
/// </summary>
public static class SendMessageRules
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
    /// Verifies that the request belongs to the given customer.
    /// </summary>
    public static void RequestShouldBelongToCustomer(CustomerRequest customerRequest, int customerId)
    {
        if (customerRequest.CustomerId != customerId)
        {
            throw new RequestNotBelongToCustomerException();
        }
    }

    /// <summary>
    /// Verifies that the request is not completed.
    /// </summary>
    public static void RequestShouldNotBeCompleted(CustomerRequest customerRequest)
    {
        if (customerRequest.StatusId == RequestStatusConstants.Completed)
        {
            throw new RequestCompletedException("Tamamlanmış bir talebe mesaj gönderilemez.");
        }
    }
}
