using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Auth.Queries.UserLogin;

/// <summary>
/// Rules belonging only to the staff login slice.
/// </summary>
public static class UserLoginRules
{
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
    /// Verifies the user is active.
    /// </summary>
    public static void UserShouldBeActive(User user)
    {
        if (!user.IsActive)
        {
            throw new UserInactiveException();
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

    /// <summary>
    /// Verifies the user has at least one role assigned (fail-closed).
    /// </summary>
    public static void UserShouldHaveRole(string? primaryRole)
    {
        if (primaryRole == null)
        {
            throw new UserHasNoRoleException();
        }
    }
}
