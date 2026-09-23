using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerRequestById;

/// <summary>
/// Rules belonging only to the customer request detail slice.
/// </summary>
public static class GetCustomerRequestByIdRules
{
    /// <summary>
    /// Verifies that the request belonging to the customer exists. Since the query is already
    /// filtered by CustomerId, existence and ownership are combined into a single message.
    /// </summary>
    public static void RequestShouldExistForCustomer(CustomerRequest? customerRequest)
    {
        if (customerRequest == null || customerRequest.IsDeleted)
        {
            throw new RequestNotBelongToCustomerException();
        }
    }
}
