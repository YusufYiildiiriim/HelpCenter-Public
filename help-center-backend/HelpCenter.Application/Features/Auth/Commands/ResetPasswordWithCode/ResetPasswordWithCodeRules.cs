using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Auth.Commands.ResetPasswordWithCode;

/// <summary>
/// Rules belonging only to the reset password with code slice.
/// </summary>
public static class ResetPasswordWithCodeRules
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
    /// Verifies the single-use password reset code in the cache is valid and matches
    /// the entered code.
    /// </summary>
    public static void ResetCodeShouldBeValid(string? cachedCode, string? providedCode)
    {
        if (string.IsNullOrEmpty(cachedCode))
        {
            throw new ResetCodeExpiredException();
        }

        if (!string.Equals(cachedCode.Trim(), providedCode?.Trim(), StringComparison.Ordinal))
        {
            throw new InvalidResetCodeException();
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
}
