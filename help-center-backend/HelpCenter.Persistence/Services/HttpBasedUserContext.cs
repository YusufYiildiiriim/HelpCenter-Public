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

    public int UserId
    {
        get
        {
            var claim = User?.FindFirst("UserId")?.Value ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }

    public IReadOnlyList<int> RoleIds
    {
        get
        {
            var user = User;
            if (user == null) return Array.Empty<int>();

            return user.FindAll("RoleId")
                .Select(c => int.TryParse(c.Value, out var id) ? id : 0)
                .Where(id => id > 0)
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
        || User?.HasClaim(c => c.Type == "CompanyId") == true;
}
