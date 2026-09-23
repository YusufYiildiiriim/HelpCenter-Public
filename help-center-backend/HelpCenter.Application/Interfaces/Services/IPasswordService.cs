namespace HelpCenter.Application.Interfaces;

/// <summary>
/// Password hashing and verification service.
/// Argon2id is the new default; BCrypt hashes are still verified for backward compatibility and
/// are upgraded to Argon2 via <see cref="NeedsRehash"/> after a successful login
/// ("opportunistic rehash" pattern).
/// </summary>
public interface IPasswordService
{
    /// <summary>Hashes new passwords with Argon2id.</summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies that the given plain text matches the stored hash.
    /// Automatically detects the hash format (Argon2id or BCrypt).
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// Returns <c>true</c> if the stored hash was produced with the older algorithm.
    /// The login flow re-hashes and updates the password when it sees this.
    /// </summary>
    bool NeedsRehash(string hashedPassword);
}
