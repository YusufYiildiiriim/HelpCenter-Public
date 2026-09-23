using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HelpCenter.Infrastructure.Services.Jwt;

/// <summary>
/// JWT access token + rotating refresh token service.
/// - Access token is short-lived (default 15 minutes).
/// - Refresh token is stored only as a SHA-256 hash.
/// - Rotation: on every use, the old token is revoked and a new one is issued.
/// - Reuse detection: if a revoked token shows up a second time, the entire chain is revoked.
/// </summary>
public class AuthTokenService : IAuthTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthTokenService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthTokens> IssueTokensAsync(int userId, string role, string email, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var (accessToken, accessExp) = GenerateAccessToken(userId, role, email);
        var (rawRefresh, hash, refreshExp) = GenerateRefreshToken();

        await _unitOfWork.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            UserId = userId,
            TokenHash = hash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExp,
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        }, ct);
        await _unitOfWork.SaveAsync(ct);

        return new AuthTokens(accessToken, accessExp, rawRefresh, refreshExp);
    }

    public async Task<AuthTokens> IssueCustomerTokensAsync(int customerId, int companyId, string email, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var (accessToken, accessExp) = GenerateAccessToken(customerId, "Customer", email, companyId.ToString());
        var (rawRefresh, hash, refreshExp) = GenerateRefreshToken();

        await _unitOfWork.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            CustomerId = customerId,
            TokenHash = hash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExp,
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        }, ct);
        await _unitOfWork.SaveAsync(ct);

        return new AuthTokens(accessToken, accessExp, rawRefresh, refreshExp);
    }

    public async Task<AuthTokens> RefreshAsync(string rawRefreshToken, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            throw new UnauthorizedException("Refresh token gerekli.");

        var hash = HashToken(rawRefreshToken);
        var stored = await _unitOfWork.Repository<RefreshToken>().FirstOrDefaultWithIncludesAsync(
            t => t.TokenHash == hash,
            new[] { "User.Account", "Customer.Account", "Customer.Company" },
            asTracking: true,
            cancellationToken: ct);

        if (stored is null)
            throw new UnauthorizedException("Refresh token geçersiz.");

        // REUSE detection: if a revoked token is presented again, revoke the entire chain.
        if (stored.IsRevoked)
        {
            await RevokeChainAsync(stored, "Reuse detected", ipAddress, ct);
            throw new UnauthorizedException("Refresh token yeniden kullanıldı. Tüm oturumlar sonlandırıldı.");
        }

        if (stored.IsExpired)
            throw new UnauthorizedException("Refresh token süresi doldu.");

        // Rotate: revoke the old token, issue a new one, and link them.
        var (rawNew, hashNew, refreshExp) = GenerateRefreshToken();

        stored.RevokedAt = DateTime.UtcNow;
        stored.RevokedReason = "Rotated";
        stored.RevokedByIp = ipAddress;
        stored.ReplacedByTokenHash = hashNew;

        await _unitOfWork.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            UserId = stored.UserId,
            CustomerId = stored.CustomerId,
            TokenHash = hashNew,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExp,
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        }, ct);

        string accessToken;
        DateTime accessExp;
        if (stored.Customer is not null)
        {
            if (!stored.Customer.IsActive || stored.Customer.Account is null || stored.Customer.Company is null)
                throw new UnauthorizedException("Customer session is no longer valid.");

            (accessToken, accessExp) = GenerateAccessToken(
                stored.Customer.Id,
                role: "Customer",
                email: stored.Customer.Account.Email,
                companyId: stored.Customer.CompanyId.ToString());
        }
        else if (stored.User is not null)
        {
            var userRoles = await _unitOfWork.Repository<UserRole>().FindAsync(
                ur => ur.UserId == stored.UserId, ct, ur => ur.Role);

            var primaryRole = userRoles
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role.Name)
                .FirstOrDefault() ?? throw new UserHasNoRoleException();

            (accessToken, accessExp) = GenerateAccessToken(
                stored.User.Id,
                role: primaryRole,
                email: stored.User.Account?.Email ?? string.Empty);
        }
        else
        {
            throw new UnauthorizedException("Refresh token principal is invalid.");
        }

        await _unitOfWork.SaveAsync(ct);
        return new AuthTokens(accessToken, accessExp, rawNew, refreshExp);
    }

    public async Task RevokeAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken)) return;

        var hash = HashToken(rawRefreshToken);
        var stored = await _unitOfWork.Repository<RefreshToken>().FirstOrDefaultAsync(
            t => t.TokenHash == hash,
            asTracking: true,
            cancellationToken: ct);

        if (stored is null) return;

        await RevokeChainAsync(stored, "Logout", ipAddress, ct);
    }

    // ----- Internals -----

    private (string token, DateTime expires) GenerateAccessToken(int principalId, string role, string email, string? companyId = null)
    {
        var jwt = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));

        // Access token lifetime is short; refresh token is long. Config can override.
        var minutes = int.TryParse(jwt["AccessTokenMinutes"], out var m) ? m : 15;
        var expires = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.Role, role),
            new("UserId", principalId.ToString()),
            new(ClaimTypes.NameIdentifier, principalId.ToString())
        };

        if (!string.IsNullOrEmpty(companyId))
            claims.Add(new Claim("CompanyId", companyId));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private (string raw, string hash, DateTime expires) GenerateRefreshToken()
    {
        var days = int.TryParse(_configuration["JwtSettings:RefreshTokenDays"], out var d) ? d : 7;
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (raw, HashToken(raw), DateTime.UtcNow.AddDays(days));
    }

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }

    private async Task RevokeChainAsync(RefreshToken token, string reason, string? ip, CancellationToken ct)
    {
        var active = await _unitOfWork.Repository<RefreshToken>().FindAsync(
            t => t.RevokedAt == null &&
                ((token.UserId != null && t.UserId == token.UserId) ||
                 (token.CustomerId != null && t.CustomerId == token.CustomerId)),
            ct);

        foreach (var t in active)
        {
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedReason = reason;
            t.RevokedByIp = ip;
            await _unitOfWork.Repository<RefreshToken>().UpdateAsync(t, ct);
        }
        await _unitOfWork.SaveAsync(ct);
    }
}
