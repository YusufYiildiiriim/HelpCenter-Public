namespace HelpCenter.Application.Common.Security;

/// <summary>Custom claim names shared by token issuers and HTTP/SignalR consumers.</summary>
public static class AppClaimTypes
{
    public const string UserId = "UserId";
    public const string RoleId = "RoleId";
    public const string CompanyId = "CompanyId";
    public const string Email = "Email";
}
