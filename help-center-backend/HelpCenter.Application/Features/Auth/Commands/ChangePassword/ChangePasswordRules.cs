using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Auth.Commands.ChangePassword;

/// <summary>
/// Rules belonging only to the change password slice.
/// </summary>
public static class ChangePasswordRules
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
    /// Verifies the user exists.
    /// </summary>
    public static void UserShouldExist(User? user)
    {
        if (user == null)
        {
            throw new UserNotFoundException();
        }
    }
}
