namespace HelpCenter.Application.Features.Roles.Queries.GetRolePermissions;

public class RolePermissionDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string ResourceKey { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = new();
    public string? AllowedFieldsJson { get; set; }
}
