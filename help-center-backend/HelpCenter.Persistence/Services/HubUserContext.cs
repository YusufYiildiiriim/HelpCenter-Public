using HelpCenter.Application.Common.Security;
using System.Security.Claims;
using HelpCenter.Application.Interfaces;

namespace HelpCenter.Persistence.Services;

/// <summary>
/// IUserContext implementation for the SignalR Hub context. IHttpContextAccessor no longer carries the
/// HttpContext in method calls made AFTER a Hub connection is established (invocations the client makes
/// over the WebSocket) — so HttpBasedUserContext always appears "anonymous" (IsAuthenticated=false, UserId=0)
/// when called inside a Hub.
/// This implementation directly uses the Hub-specific ClaimsPrincipal
/// (HubCallerContext.User) that SignalR keeps alive for the duration of the connection; it does not depend on HttpContext.
/// </summary>
public class HubUserContext : IUserContext
{
    private readonly ClaimsPrincipal? _user;

    public HubUserContext(ClaimsPrincipal? user)
    {
        _user = user;
    }

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;

    public Guid? CompanyId => Guid.TryParse(_user?.FindFirst(AppClaimTypes.CompanyId)?.Value, out var id) && id != Guid.Empty ? id : null;

    public Guid UserId
    {
        get
        {
            var claim = _user?.FindFirst(AppClaimTypes.UserId)?.Value ?? _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }

    public IReadOnlyList<Guid> RoleIds
    {
        get
        {
            if (_user == null) return Array.Empty<Guid>();

            return _user.FindAll(AppClaimTypes.RoleId)
                .Select(c => Guid.TryParse(c.Value, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToArray();
        }
    }

    public string? UserName =>
        _user?.Identity?.Name
        ?? _user?.FindFirst(ClaimTypes.Email)?.Value
        ?? _user?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Role => _user?.FindFirst(ClaimTypes.Role)?.Value;

    /// <summary>Per-request IP tracking is not done in the Hub context; RequestHub does not use this.</summary>
    public string ClientIp => "Unknown";

    /// <summary>Per-request User-Agent tracking is not done in the Hub context; RequestHub does not use this.</summary>
    public string? UserAgent => null;

    public bool IsCustomer =>
        string.Equals(Role, "Customer", StringComparison.OrdinalIgnoreCase)
        || _user?.HasClaim(c => c.Type == AppClaimTypes.CompanyId) == true;
}
