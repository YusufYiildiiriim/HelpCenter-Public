using HelpCenter.Application.Common.Security;
using System.Security.Claims;
using HelpCenter.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HelpCenter.Persistence.Services;

public class HttpBasedUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpBasedUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? CompanyId => Guid.TryParse(User?.FindFirst(AppClaimTypes.CompanyId)?.Value, out var id) && id != Guid.Empty ? id : null;

    public Guid UserId
    {
        get
        {
            var claim = User?.FindFirst(AppClaimTypes.UserId)?.Value ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }

    public IReadOnlyList<Guid> RoleIds
    {
        get
        {
            var user = User;
            if (user == null) return Array.Empty<Guid>();

            return user.FindAll(AppClaimTypes.RoleId)
                .Select(c => Guid.TryParse(c.Value, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToArray();
        }
    }

    public string? UserName =>
        User?.Identity?.Name
        ?? User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

    public string ClientIp =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

    public bool IsCustomer =>
        string.Equals(Role, "Customer", StringComparison.OrdinalIgnoreCase)
        || User?.HasClaim(c => c.Type == AppClaimTypes.CompanyId) == true;
}
