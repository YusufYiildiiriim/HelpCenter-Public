using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Customers.Commands.DeleteCustomer;

/// <summary>
/// Rules belonging only to the delete customer slice.
/// </summary>
public static class DeleteCustomerRules
{
    /// <summary>
    /// Verifies the customer exists.
    /// </summary>
    public static void CustomerShouldExist(Customer? customer)
    {
        if (customer == null || customer.IsDeleted)
        {
            throw new CustomerNotFoundException();
        }
    }
}
