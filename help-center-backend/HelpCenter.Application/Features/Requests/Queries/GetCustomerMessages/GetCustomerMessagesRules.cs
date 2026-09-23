using HelpCenter.Application.Exceptions;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerMessages;

/// <summary>
/// Rules belonging only to the customer message listing slice.
/// </summary>
public static class GetCustomerMessagesRules
{
    /// <summary>
    /// Verifies that the request exists in the system. A separate `IsDeleted` check is not needed —
    /// the projection query already goes through the default (filtered) query.
    /// </summary>
    public static void RequestShouldExist(CustomerMessagesTicketSummary? ticket)
    {
        if (ticket == null)
        {
            throw new RequestNotFoundException();
        }
    }

    /// <summary>
    /// Verifies that the request belongs to the given customer.
    /// </summary>
    public static void RequestShouldBelongToCustomer(CustomerMessagesTicketSummary ticket, int customerId)
    {
        if (ticket.CustomerId != customerId)
        {
            throw new RequestNotBelongToCustomerException();
        }
    }
}
