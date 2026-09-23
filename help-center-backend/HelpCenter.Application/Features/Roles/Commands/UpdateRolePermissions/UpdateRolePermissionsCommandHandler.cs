using HelpCenter.Application.Interfaces;
using MediatR;

namespace HelpCenter.Application.Features.Roles.Commands.UpdateRolePermissions;

public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, bool>
{
    private readonly IRolePermissionSyncService _rolePermissionSyncService;
    private readonly IAuditLogWriter _auditLog;

    public UpdateRolePermissionsCommandHandler(IRolePermissionSyncService rolePermissionSyncService, IAuditLogWriter auditLog)
    {
        _rolePermissionSyncService = rolePermissionSyncService;
        _auditLog = auditLog;
    }

    public async Task<bool> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var permissions = request.Permissions.Select(p =>
        {
            var actions = p.Actions?.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList() ?? new List<string>();
            return (p.ResourceKey, (IReadOnlyList<string>)actions, p.AllowedFieldsJson);
        }).ToList();

        // The Include chain ("RolePermissions.Actions") + tracking + saving now live in
        // IRolePermissionSyncService (Persistence layer).
        var synced = await _rolePermissionSyncService.SyncPermissionsAsync(request.RoleId, permissions, cancellationToken);

        UpdateRolePermissionsRules.RoleShouldExist(synced);

        await _auditLog.WriteAsync("RolePermissionsUpdated", "Role", request.RoleId, new { Permissions = request.Permissions }, cancellationToken);

        return true;
    }
}
