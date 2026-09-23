using HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;

namespace HelpCenter.Application.Features.Auth.Queries.GetVerifyState;

public class GetVerifyStateResponse
{
    public bool IsPasswordChangeRequired { get; set; }
    public UserPermissionsDto Permissions { get; set; } = new();
}
