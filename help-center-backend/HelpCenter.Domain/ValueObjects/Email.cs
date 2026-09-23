using System.Text.RegularExpressions;

namespace HelpCenter.Domain.ValueObjects;

public readonly record struct Email
{
    public string Value { get; }

    private Email(string v) => Value = v;

    public static Email Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Email boş olamaz.", nameof(raw));

        var trimmed = raw.Trim().ToLowerInvariant();

        if (!Regex.IsMatch(trimmed, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.None, TimeSpan.FromMilliseconds(200)))
            throw new ArgumentException($"Geçersiz email: {raw}", nameof(raw));

        if (trimmed.Length > 254)
            throw new ArgumentException("Email 254 karakteri aşamaz.", nameof(raw));

        return new Email(trimmed);
    }

    public override string ToString() => Value;
    public static implicit operator string(Email e) => e.Value;
}
