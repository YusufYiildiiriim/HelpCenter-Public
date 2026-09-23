using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Roles.Commands.DeleteRole;

/// <summary>
/// Rules belonging only to the role deletion slice.
/// </summary>
public static class DeleteRoleRules
{
    /// <summary>
    /// Verifies that the role exists in the system.
    /// </summary>
    public static void RoleShouldExist(Role? role)
    {
        if (role == null || role.IsDeleted)
        {
            throw new RoleNotFoundException();
        }
    }

    /// <summary>
    /// Verifies that a role assigned to users cannot be deleted.
    /// </summary>
    public static void RoleShouldNotBeAssignedToUsers(bool isAssigned)
    {
        if (isAssigned)
        {
            throw new RoleAssignedToUsersException();
        }
    }
}
