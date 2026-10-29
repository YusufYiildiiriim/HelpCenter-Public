namespace HelpCenter.Application.Features.Roles.Queries.GetRoles;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public Guid RowVersion { get; set; }
    public bool HasUsers { get; set; }
    public int PermissionCount { get; set; }
}
