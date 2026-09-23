namespace HelpCenter.Domain.Entities;

/// <summary>
/// Refresh token — only the SHA-256 hash is stored. A new token is issued on every
/// use (rotation), and the old one is marked with <see cref="RevokedAt"/>.
/// If the same token shows up a second time, the entire chain is revoked (reuse detection).
/// </summary>
public class RefreshToken
{
    public long Id { get; set; }

    public int? UserId { get; set; }
    public virtual User? User { get; set; }

    public int? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    /// <summary>Base64(SHA-256(raw token)) — the plain token is never stored.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }

    /// <summary>Hash of the new token that replaced this one in the rotation chain.</summary>
    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public string? UserAgent { get; set; }

    // ----- Derived -----

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
