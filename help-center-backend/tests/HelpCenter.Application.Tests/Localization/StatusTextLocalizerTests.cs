using FluentAssertions;
using HelpCenter.Application.Features.Statuses;
using HelpCenter.Domain.Constants;
using Xunit;

namespace HelpCenter.Application.Tests.Localization;

public sealed class StatusTextLocalizerTests
{
    [Fact]
    public void Localize_returns_English_system_status_text_for_English_locale()
    {
        var result = StatusTextLocalizer.Localize(RequestStatusConstants.WaitingForReply, "Cevap Bekliyor", "Kullanıcıdan gelen talep cevaplanmayı bekliyor.", "en-US");

        result.Name.Should().Be("Waiting for reply");
        result.Description.Should().Be("The request is waiting for a response from support.");
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("fr-FR")]
    public void Localize_preserves_source_text_for_non_English_or_unsupported_locale(string locale)
    {
        var result = StatusTextLocalizer.Localize(RequestStatusConstants.WaitingForReply, "Cevap Bekliyor", "Kullanıcıdan gelen talep cevaplanmayı bekliyor.", locale);

        result.Name.Should().Be("Cevap Bekliyor");
        result.Description.Should().Be("Kullanıcıdan gelen talep cevaplanmayı bekliyor.");
    }

    [Fact]
    public void Localize_does_not_guess_translations_for_custom_statuses()
    {
        var result = StatusTextLocalizer.Localize(Guid.NewGuid(), "Custom", "Custom description", "en-US");

        result.Name.Should().Be("Custom");
        result.Description.Should().Be("Custom description");
    }
}
