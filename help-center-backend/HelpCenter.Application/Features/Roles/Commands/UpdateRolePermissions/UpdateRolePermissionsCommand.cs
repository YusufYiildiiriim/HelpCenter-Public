using MediatR;

namespace HelpCenter.Application.Features.Roles.Commands.UpdateRolePermissions;

public class UpdateRolePermissionsCommand : IRequest<bool>
{
    public int RoleId { get; set; }
    public List<UpdateRolePermissionDto> Permissions { get; set; } = new();
}

public class UpdateRolePermissionDto
{
    public string ResourceKey { get; set; } = string.Empty;

    /// <summary>
    /// Actions granted for this resource (source of truth).
    /// E.g.: ["Read", "Create", "ManageMembers"].
    /// An empty list means no permissions at all for that resource (soft-deleted).
    /// </summary>
    public List<string> Actions { get; set; } = new();

    public string? AllowedFieldsJson { get; set; }
}
