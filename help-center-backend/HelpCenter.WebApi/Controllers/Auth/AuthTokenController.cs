using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Auth;

/// <summary>
/// Refresh token lifecycle. Refresh credentials are carried only in the HttpOnly cookie.
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthTokenController : ControllerBase
{
    public const string RefreshTokenCookieName = "refresh_token";
    private readonly IAuthTokenService _tokens;

    public AuthTokenController(IAuthTokenService tokens) => _tokens = tokens;

    /// <summary>Rotates the refresh token and returns a new access token.</summary>
    /// <remarks>If reuse is detected, the entire chain is revoked (theft protection).</remarks>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("session-refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokens>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = HttpContext.Request.Headers.UserAgent.ToString();
        var tokens = await _tokens.RefreshAsync(Request.Cookies[RefreshTokenCookieName] ?? string.Empty, ip, ua, ct);
        SetRefreshCookie(tokens);
        return Ok(ApiResponse<AuthTokens>.SuccessResult(tokens));
    }

    /// <summary>Revokes the refresh token (logout).</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [EnableRateLimiting("session-logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _tokens.RevokeAsync(Request.Cookies[RefreshTokenCookieName] ?? string.Empty, ip, ct);
        Response.Cookies.Delete(RefreshTokenCookieName, RefreshCookieOptions());
        return Ok(ApiResponse.Ok("Oturum sonlandırıldı."));
    }

    private void SetRefreshCookie(AuthTokens tokens)
    {
        var options = RefreshCookieOptions();
        options.Expires = tokens.RefreshTokenExpiresAt;
        Response.Cookies.Append(RefreshTokenCookieName, tokens.RefreshToken, options);
    }

    internal static CookieOptions RefreshCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Path = "/"
    };
}
