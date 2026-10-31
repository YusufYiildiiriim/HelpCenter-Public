using FluentAssertions;
using HelpCenter.Application.Common.Models;
using HelpCenter.Domain.Common;
using Xunit;

namespace HelpCenter.Application.Tests.Localization;

public sealed class LocaleRulesTests
{
    [Theory]
    [InlineData("tr", SupportedLocales.Turkish)]
    [InlineData("tr-TR", SupportedLocales.Turkish)]
    [InlineData("EN", SupportedLocales.English)]
    [InlineData("en-US", SupportedLocales.English)]
    public void Normalize_returns_canonical_supported_locale(string value, string expected)
        => SupportedLocales.Normalize(value).Should().Be(expected);

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("")]
    [InlineData("en-GB")]
    public void Normalize_rejects_unsupported_locale(string value)
        => SupportedLocales.Normalize(value).Should().BeNull();

    [Fact]
    public void Translation_rules_reject_duplicate_canonical_locales()
    {
        var values = new[]
        {
            new LocalizedTextInput { Locale = "en", Title = "Title", Description = "Text" },
            new LocalizedTextInput { Locale = "en-US", Title = "Title 2", Description = "Text 2" }
        };

        LocalizedTextRules.HasValidLocales(values).Should().BeFalse();
    }

    [Fact]
    public void Translation_rules_reject_incomplete_or_unsupported_translation()
    {
        LocalizedTextRules.HasValidLocales(new[]
        {
            new LocalizedTextInput { Locale = "en-US", Title = "Title", Description = "" }
        }).Should().BeFalse();
        LocalizedTextRules.HasValidLocales(new[]
        {
            new LocalizedTextInput { Locale = "de-DE", Title = "Title", Description = "Text" }
        }).Should().BeFalse();
    }

    [Fact]
    public void Translation_rules_accept_one_complete_translation_per_locale()
    {
        LocalizedTextRules.HasValidLocales(new[]
        {
            new LocalizedTextInput { Locale = "tr-TR", Title = "Başlık", Description = "Metin" },
            new LocalizedTextInput { Locale = "en-US", Title = "Title", Description = "Text" }
        }).Should().BeTrue();
    }
}
