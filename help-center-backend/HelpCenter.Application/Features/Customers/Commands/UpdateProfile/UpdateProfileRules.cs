using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Customers.Commands.UpdateProfile;

/// <summary>
/// Rules belonging only to the update customer profile slice.
/// </summary>
public static class UpdateProfileRules
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
