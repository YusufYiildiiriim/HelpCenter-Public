using System.Security.Cryptography;
using System.Text;
using HelpCenter.Application.Interfaces;
using Konscious.Security.Cryptography;
using HelpCenter.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace HelpCenter.Infrastructure.Services;

/// <summary>
/// Dual-format password service supporting Argon2id (new default) + BCrypt (legacy).
/// Parameters are configured through PasswordHashing; defaults remain 64 MiB, 3 iterations, parallelism 4.
/// Encoded format: <c>$argon2id$v=19$m=65536,t=3,p=4$&lt;salt-b64&gt;$&lt;hash-b64&gt;</c>.
/// </summary>
public class PasswordService : IPasswordService
{
    private const int SaltSize = 16;          // 128 bit
    private const int HashSize = 32;          // 256 bit
    private readonly PasswordHashOptions _options;

    public PasswordService() : this(Options.Create(new PasswordHashOptions())) { }

    public PasswordService(IOptions<PasswordHashOptions> options)
    {
        _options = options.Value;
        Validator.ValidateObject(_options, new ValidationContext(_options), validateAllProperties: true);
    }

    private const string Argon2Prefix = "$argon2id$";
    private const string BCryptPrefix2A = "$2a$";
    private const string BCryptPrefix2B = "$2b$";
    private const string BCryptPrefix2Y = "$2y$";

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Argon2ComputeHash(password, salt);

        return $"$argon2id$v=19$m={_options.MemorySizeKb},t={_options.Iterations},p={_options.Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
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
            return !TryParseArgon2(hashedPassword, out var encoded)
                || encoded.Memory != _options.MemorySizeKb
                || encoded.Iterations != _options.Iterations
                || encoded.Parallelism != _options.Parallelism
                || encoded.Salt.Length != SaltSize || encoded.Hash.Length != HashSize;
        }

        return true;
    }

    // --- Internals ---

    private byte[] Argon2ComputeHash(string password, byte[] salt)
    {
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = _options.Parallelism,
            MemorySize = _options.MemorySizeKb,
            Iterations = _options.Iterations
        };
        return argon.GetBytes(HashSize);
    }

    private static bool VerifyArgon2(string password, string encoded)
    {
        if (!TryParseArgon2(encoded, out var parsed)) return false;
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = parsed.Salt,
            DegreeOfParallelism = parsed.Parallelism,
            MemorySize = parsed.Memory,
            Iterations = parsed.Iterations
        };
        var actual = argon.GetBytes(parsed.Hash.Length);
        return CryptographicOperations.FixedTimeEquals(actual, parsed.Hash);
    }

    private sealed record EncodedHash(int Memory, int Iterations, int Parallelism, byte[] Salt, byte[] Hash);

    private static bool TryParseArgon2(string encoded, out EncodedHash parsed)
    {
        parsed = null!;
        var parts = encoded.Split('$', StringSplitOptions.RemoveEmptyEntries);
        // ["argon2id", "v=19", "m=...,t=...,p=...", salt, hash]
        if (parts.Length != 5 || parts[0] != "argon2id" || parts[1] != "v=19") return false;

        var paramSegment = parts[2];
        var parameters = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var kv in paramSegment.Split(','))
        {
            var eq = kv.IndexOf('=');
            if (eq <= 0) return false;
            var k = kv[..eq];
            var v = kv[(eq + 1)..];
            if (!int.TryParse(v, out var n) || !parameters.TryAdd(k, n)) return false;
        }

        if (parameters.Count != 3 || !parameters.TryGetValue("m", out var memory)
            || !parameters.TryGetValue("t", out var iterations) || !parameters.TryGetValue("p", out var parallelism)
            || parallelism is < 1 or > 32 || iterations is < 1 or > 20
            || memory < 8 * parallelism || memory > 1048576) return false;

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expected = Convert.FromBase64String(parts[4]);
        }
        catch { return false; }

        if (salt.Length is < 8 or > 64 || expected.Length is < 16 or > 64) return false;
        parsed = new EncodedHash(memory, iterations, parallelism, salt, expected);
        return true;
    }

    private static bool IsBCrypt(string hash)
        => hash.StartsWith(BCryptPrefix2A, StringComparison.Ordinal)
        || hash.StartsWith(BCryptPrefix2B, StringComparison.Ordinal)
        || hash.StartsWith(BCryptPrefix2Y, StringComparison.Ordinal);
}
