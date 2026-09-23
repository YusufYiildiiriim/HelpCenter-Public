using System.Security.Cryptography;
using System.Text;
using HelpCenter.Application.Interfaces;
using Konscious.Security.Cryptography;

namespace HelpCenter.Infrastructure.Services;

/// <summary>
/// Dual-format password service supporting Argon2id (new default) + BCrypt (legacy).
/// Parameters follow OWASP 2024 recommendations: memory 64 MiB, iterations 3, parallelism 4.
/// Encoded format: <c>$argon2id$v=19$m=65536,t=3,p=4$&lt;salt-b64&gt;$&lt;hash-b64&gt;</c>.
/// </summary>
public class PasswordService : IPasswordService
{
    private const int SaltSize = 16;          // 128 bit
    private const int HashSize = 32;          // 256 bit
    private const int Iterations = 3;
    private const int MemorySizeKb = 64 * 1024;   // 64 MiB
    private const int Parallelism = 4;

    private const string Argon2Prefix = "$argon2id$";
    private const string BCryptPrefix2A = "$2a$";
    private const string BCryptPrefix2B = "$2b$";
    private const string BCryptPrefix2Y = "$2y$";

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Argon2ComputeHash(password, salt);

        return $"$argon2id$v=19$m={MemorySizeKb},t={Iterations},p={Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        if (hashedPassword.StartsWith(Argon2Prefix, StringComparison.Ordinal))
            return VerifyArgon2(password, hashedPassword);

        if (IsBCrypt(hashedPassword))
        {
            try { return BCrypt.Net.BCrypt.Verify(password, hashedPassword); }
            catch { return false; }
        }

        // Unknown format — stay on the safe side.
        return false;
    }

    public bool NeedsRehash(string hashedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            return true;

        // BCrypt hashes should be upgraded.
        if (IsBCrypt(hashedPassword))
            return true;

        // Rehash if Argon2id parameters changed.
        if (hashedPassword.StartsWith(Argon2Prefix, StringComparison.Ordinal))
        {
            var expected = $"m={MemorySizeKb},t={Iterations},p={Parallelism}";
            return !hashedPassword.Contains(expected, StringComparison.Ordinal);
        }

        return true;
    }

    // --- Internals ---

    private static byte[] Argon2ComputeHash(string password, byte[] salt)
    {
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = Parallelism,
            MemorySize = MemorySizeKb,
            Iterations = Iterations
        };
        return argon.GetBytes(HashSize);
    }

    private static bool VerifyArgon2(string password, string encoded)
    {
        var parts = encoded.Split('$', StringSplitOptions.RemoveEmptyEntries);
        // ["argon2id", "v=19", "m=...,t=...,p=...", salt, hash]
        if (parts.Length != 5) return false;

        var paramSegment = parts[2];
        int memory = MemorySizeKb, iterations = Iterations, parallelism = Parallelism;
        foreach (var kv in paramSegment.Split(','))
        {
            var eq = kv.IndexOf('=');
            if (eq <= 0) continue;
            var k = kv[..eq];
            var v = kv[(eq + 1)..];
            if (!int.TryParse(v, out var n)) continue;
            switch (k)
            {
                case "m": memory = n; break;
                case "t": iterations = n; break;
                case "p": parallelism = n; break;
            }
        }

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expected = Convert.FromBase64String(parts[4]);
        }
        catch { return false; }

        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = parallelism,
            MemorySize = memory,
            Iterations = iterations
        };
        var actual = argon.GetBytes(expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static bool IsBCrypt(string hash)
        => hash.StartsWith(BCryptPrefix2A, StringComparison.Ordinal)
        || hash.StartsWith(BCryptPrefix2B, StringComparison.Ordinal)
        || hash.StartsWith(BCryptPrefix2Y, StringComparison.Ordinal);
}
