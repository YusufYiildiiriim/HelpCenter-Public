using System.ComponentModel.DataAnnotations;

namespace HelpCenter.WebApi.Configuration;

public sealed class SmtpOptions
{
    public const string SectionName = "SmtpSettings";

    [Required]
    public string Host { get; init; } = default!;

    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    [Required]
    public string Username { get; init; } = default!;

    [Required]
    public string Password { get; init; } = default!;

    [Required, EmailAddress]
    public string FromAddress { get; init; } = default!;
}
