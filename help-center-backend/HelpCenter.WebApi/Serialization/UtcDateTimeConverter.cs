using System.Text.Json;
using System.Text.Json.Serialization;

namespace HelpCenter.WebApi.Serialization;

// SQL datetime2 loses Kind; persisted application timestamps represent UTC.
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
        AsUtc(reader.GetDateTime());

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(AsUtc(value));

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => value
    };
}