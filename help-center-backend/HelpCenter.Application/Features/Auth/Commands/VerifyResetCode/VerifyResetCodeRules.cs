using HelpCenter.Application.Exceptions;

namespace HelpCenter.Application.Features.Auth.Commands.VerifyResetCode;

/// <summary>
/// Rules belonging only to the reset code verification slice.
/// </summary>
public static class VerifyResetCodeRules
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
}
