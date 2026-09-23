using System.ComponentModel.DataAnnotations;

namespace HelpCenter.WebApi.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string ConnectionString { get; init; } = default!;

    [Range(1, 600)]
    public int CommandTimeoutSeconds { get; init; } = 30;

    public bool AutoMigrate { get; init; }
}
