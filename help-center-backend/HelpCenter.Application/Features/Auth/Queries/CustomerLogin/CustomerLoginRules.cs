using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Auth.Queries.CustomerLogin;

/// <summary>
/// Rules belonging only to the customer login slice.
/// </summary>
public static class CustomerLoginRules
{
    /// <summary>
    /// Verifies the customer exists.
    /// </summary>
    public static void CustomerShouldExist(Customer? customer)
    {
        if (customer == null)
        {
            throw new CustomerNotFoundException();
        }
    }

    /// <summary>
    /// Verifies the customer is active.
    /// </summary>
    public static void CustomerShouldBeActive(Customer customer)
    {
        if (!customer.IsActive)
        {
            throw new CustomerInactiveException();
        }
    }

    /// <summary>
    /// Verifies the password is correct.
    /// </summary>
    public static void PasswordShouldMatch(bool isValidPassword)
    {
        if (!isValidPassword)
        {
            throw new InvalidCredentialsException();
        }
    }
}
