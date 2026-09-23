using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Commands.CloseRequest;

/// <summary>
/// Rules belonging only to the customer close-request slice.
/// </summary>
public static class CloseRequestRules
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
}
