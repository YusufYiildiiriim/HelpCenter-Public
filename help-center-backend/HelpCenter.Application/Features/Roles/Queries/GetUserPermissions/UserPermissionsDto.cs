namespace HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;

public class UserPermissionsDto
{
    public List<ModulePermissionDto> Modules { get; set; } = new();
}

public class ModulePermissionDto
{
    public string ResourceKey { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = new();
}
