namespace HelpCenter.Domain.Common;

public static class SupportedLocales
{
    public const string Turkish = "tr-TR";
    public const string English = "en-US";
    public const string Default = Turkish;

    public static string? Normalize(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale)) return null;
        return locale.Trim().ToLowerInvariant() switch
        {
            "tr" or "tr-tr" => Turkish,
            "en" or "en-us" => English,
            _ => null
        };
    }

    public static bool IsSupported(string? locale) => Normalize(locale) is not null;
}