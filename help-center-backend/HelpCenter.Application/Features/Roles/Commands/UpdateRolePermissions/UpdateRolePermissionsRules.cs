using HelpCenter.Application.Exceptions;

namespace HelpCenter.Application.Features.Roles.Commands.UpdateRolePermissions;

/// <summary>
/// Rules belonging only to the role permissions update slice.
/// </summary>
public static class UpdateRolePermissionsRules
{
    /// <summary>
    /// Verifies that the role exists in the system. `IRolePermissionSyncService.SyncPermissionsAsync`
    /// returns `false` if the role is not found or deleted (the IsDeleted check is also done inside the service).
    /// </summary>
    public static void RoleShouldExist(bool roleFoundAndSynced)
    {
        if (!roleFoundAndSynced)
        {
            throw new RoleNotFoundException();
        }
    }
}
