using System.ComponentModel.DataAnnotations;

namespace HelpCenter.Infrastructure.Configuration;

public sealed class PasswordHashOptions
{
    public const string SectionName = "PasswordHashing";

    [Range(8192, 1048576)]
    public int MemorySizeKb { get; set; } = 65536;

    [Range(1, 20)]
    public int Iterations { get; set; } = 3;

    [Range(1, 32)]
    public int Parallelism { get; set; } = 4;
}
