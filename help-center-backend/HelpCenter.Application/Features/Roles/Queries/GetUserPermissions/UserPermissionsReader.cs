using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;

/// <summary>
/// Builds the permission list derived from the user's roles. Both
/// <see cref="GetUserPermissionsQueryHandler"/> and other handlers that need this
/// information read from here — instead of a handler invoking another handler (IMediator.Send),
/// the shared logic is kept in a single place.
/// </summary>
public sealed class UserPermissionsReader
{
    private readonly IUnitOfWork _unitOfWork;

    public UserPermissionsReader(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserPermissionsDto> ReadAsync(int userId, CancellationToken cancellationToken)
    {
        // Previously the full role graph was fetched with a "Role.RolePermissions.Actions" string-path
        // Include and flattened on the C# side. Now only the (ResourceKey, Actions) pairs are projected
        // directly in SQL — merging conflicting ResourceKeys coming from multiple roles (union) is still
        // done client-side (this step is unavoidable: merging permissions for the same resource coming
        // from different roles is done here, not in SQL).
        var perRoleGroups = await _unitOfWork.Repository<UserRole>().SelectAsync(
            x => x.UserId == userId && x.Role != null && x.Role.IsActive,
            x => x.Role!.RolePermissions
                .Select(rp => new
                {
                    rp.ResourceKey,
                    Actions = rp.Actions.Select(a => a.Action).ToList()
                })
                .ToList(),
            cancellationToken: cancellationToken);

        var resourcePermissions = new Dictionary<string, ModulePermissionDto>();

        foreach (var perm in perRoleGroups.SelectMany(x => x))
        {
            if (!resourcePermissions.TryGetValue(perm.ResourceKey, out var rPerm))
            {
                rPerm = new ModulePermissionDto { ResourceKey = perm.ResourceKey };
                resourcePermissions[perm.ResourceKey] = rPerm;
            }

            foreach (var action in perm.Actions)
            {
                if (!rPerm.Actions.Contains(action))
                {
                    rPerm.Actions.Add(action);
                }
            }
        }

        return new UserPermissionsDto { Modules = resourcePermissions.Values.ToList() };
    }
}
