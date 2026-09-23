using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Rules belonging only to the update customer slice.
/// </summary>
public static class UpdateCustomerRules
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

    /// <summary>
    /// Verifies the requested username has not been taken by someone else.
    /// </summary>
    public static void UsernameShouldBeAvailable(bool isTaken, string username)
    {
        if (isTaken)
        {
            throw new UsernameAlreadyExistsException($"'{username}' kullanıcı adı zaten kullanımda.");
        }
    }
}
