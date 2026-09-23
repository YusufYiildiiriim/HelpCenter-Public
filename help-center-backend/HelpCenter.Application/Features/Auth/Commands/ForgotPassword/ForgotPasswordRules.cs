using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Rules belonging only to the forgot password slice.
/// </summary>
public static class ForgotPasswordRules
{
    /// <summary>
    /// Verifies the email / username input is filled in.
    /// </summary>
    public static void IdentifierShouldBeProvided(string? identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new InvalidIdentifierException();
        }
    }

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

    /// <summary>
    /// The account must have an email so the reset code can be sent.
    /// </summary>
    public static void AccountShouldHaveEmail(string? targetEmail)
    {
        if (string.IsNullOrEmpty(targetEmail))
        {
            throw new EmailNotFoundException();
        }
    }
}
