using System.Text.Json.Serialization;

namespace HelpCenter.Application.Interfaces;

/// <summary>
/// Access token + refresh token lifecycle. The refresh token is rotated on every use;
/// if the same refresh token arrives a second time, the chain is revoked
/// (reuse detection = theft detection).
/// </summary>
public interface IAuthTokenService
{
    /// <summary>Issues a new access+refresh token pair after login.</summary>
    Task<AuthTokens> IssueTokensAsync(int userId, string role, string email, string? ipAddress, string? userAgent, CancellationToken ct = default);

    /// <summary>Issues a customer access+refresh token pair after customer login.</summary>
    Task<AuthTokens> IssueCustomerTokensAsync(int customerId, int companyId, string email, string? ipAddress, string? userAgent, CancellationToken ct = default);

    /// <summary>Rotates the refresh token: invalidates the old one, issues a new one. Revokes the chain if reuse is detected.</summary>
    Task<AuthTokens> RefreshAsync(string rawRefreshToken, string? ipAddress, string? userAgent, CancellationToken ct = default);

    /// <summary>Logout: revokes the refresh token and its chain, if any.</summary>
    Task RevokeAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default);
}

/// <summary>Token pair + expiration dates returned after login/refresh.</summary>
public sealed record AuthTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    [property: JsonIgnore]
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
