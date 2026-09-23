namespace HelpCenter.Application.Interfaces;

/// <summary>
/// Hides the bulk synchronization of a role's permissions from the Application layer — the Include
/// chain (RolePermissions.Actions) and saving (SaveChanges) live in one place here. The handler
/// just says "sync this role's permissions with these", and doesn't care about HOW it's done.
/// </summary>
public interface IRolePermissionSyncService
{
    /// <summary>
    /// Returns `false` if the role is not found (the Application/Rules layer interprets this as
    /// "not found"). If the role exists, syncs and saves its permissions, then returns `true`.
    /// </summary>
    Task<bool> SyncPermissionsAsync(
        int roleId,
        IEnumerable<(string ResourceKey, IReadOnlyList<string> Actions, string? AllowedFieldsJson)> permissions,
        CancellationToken cancellationToken = default);
}
