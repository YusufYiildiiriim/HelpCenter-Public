namespace HelpCenter.Application.Common.Models;

public class LocalizedTextInput
{
    public string Locale { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public static class LocalizedTextRules
{
    public static bool HasValidLocales(IEnumerable<LocalizedTextInput>? values)
    {
        if (values is null) return true;
        var normalized = new List<string>();
        foreach (var item in values)
        {
            var locale = HelpCenter.Domain.Common.SupportedLocales.Normalize(item.Locale);
            if (locale is null || string.IsNullOrWhiteSpace(item.Title) || string.IsNullOrWhiteSpace(item.Description)) return false;
            normalized.Add(locale);
        }
        return normalized.Distinct(StringComparer.Ordinal).Count() == normalized.Count;
    }
}
