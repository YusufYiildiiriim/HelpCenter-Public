using System.Text.Json;
using HelpCenter.WebApi.Serialization;
using Xunit;

namespace HelpCenter.Application.Tests;

public class UtcSerializationTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new UtcDateTimeConverter() } };

    [Theory]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Utc)]
    public void Stored_and_live_timestamps_have_identical_utc_wire_values(DateTimeKind kind)
    {
        var date = new DateTime(2026, 10, 11, 20, 34, 0, kind);
        Assert.Equal("\"2026-10-11T20:34:00Z\"", JsonSerializer.Serialize(date, Options));
        Assert.Equal("\"2026-10-11T20:34:00Z\"", JsonSerializer.Serialize<DateTime?>(date, Options));
    }

    [Theory]
    [InlineData("2026-10-11T20:34:00", "2026-10-11T20:34:00Z")]
    [InlineData("2026-10-11T23:34:00+03:00", "2026-10-11T20:34:00Z")]
    [InlineData("2026-10-11T16:34:00-04:00", "2026-10-11T20:34:00Z")]
    public void Reads_offset_and_legacy_values_as_same_instant(string input, string expected)
    {
        var parsed = JsonSerializer.Deserialize<DateTime>($"\"{input}\"", Options);
        Assert.Equal(DateTimeKind.Utc, parsed.Kind);
        Assert.Equal($"\"{expected}\"", JsonSerializer.Serialize(parsed, Options));
    }
}