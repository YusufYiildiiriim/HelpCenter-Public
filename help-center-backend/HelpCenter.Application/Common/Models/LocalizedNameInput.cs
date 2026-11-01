namespace HelpCenter.Application.Common.Models;

public class LocalizedNameInput
{
    public string Locale { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public static class LocalizedNameRules
{
    public static bool HasValidSourceLocale(IEnumerable<LocalizedNameInput>? values, string? sourceLocale)
    {
        if (values is null || !values.Any()) return true;
        var source = HelpCenter.Domain.Common.SupportedLocales.Normalize(sourceLocale);
        return source is not null && values.Any(item => HelpCenter.Domain.Common.SupportedLocales.Normalize(item.Locale) == source);
    }

    public static bool HasValidLocales(IEnumerable<LocalizedNameInput>? values)
    {
        if (values is null) return true;
        var normalized = new List<string>();
        foreach (var item in values)
        {
            var locale = HelpCenter.Domain.Common.SupportedLocales.Normalize(item.Locale);
            if (locale is null || string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Description)) return false;
            normalized.Add(locale);
        }
        return normalized.Distinct(StringComparer.Ordinal).Count() == normalized.Count;
    }
}
