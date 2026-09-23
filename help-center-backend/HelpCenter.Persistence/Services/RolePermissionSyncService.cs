using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Services;

public class RolePermissionSyncService : IRolePermissionSyncService
{
    private readonly EfContext _efContext;

    public RolePermissionSyncService(EfContext efContext)
    {
        _efContext = efContext;
    }

    public async Task<bool> SyncPermissionsAsync(
        int roleId,
        IEnumerable<(string ResourceKey, IReadOnlyList<string> Actions, string? AllowedFieldsJson)> permissions,
        CancellationToken cancellationToken = default)
    {
        var role = await _efContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Actions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role == null || role.IsDeleted) return false;

        role.SyncPermissions(permissions);
        await _efContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
