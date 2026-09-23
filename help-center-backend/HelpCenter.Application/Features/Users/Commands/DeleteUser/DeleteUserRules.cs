using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Users.Commands.DeleteUser;

/// <summary>
/// Rules belonging only to the user deletion slice.
/// </summary>
public static class DeleteUserRules
{
    /// <summary>
    /// Verifies that the user exists in the system.
    /// </summary>
    public static void UserShouldExist(User? user)
    {
        if (user == null || user.IsDeleted)
        {
            throw new UserNotFoundException();
        }
    }
}
