using System.ComponentModel.DataAnnotations;

namespace HelpCenter.WebApi.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "JwtSettings";

    [Required, MinLength(32)]
    public string SecretKey { get; init; } = default!;

    [Required]
    public string Issuer { get; init; } = default!;

    [Required]
    public string Audience { get; init; } = default!;

    [Range(1, 60)]
    public int AccessTokenMinutes { get; init; } = 15;

    [Range(1, 90)]
    public int RefreshTokenDays { get; init; } = 7;
}
