using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolCRM.API.Converters;

public sealed class TimeSpanJsonConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return default;

        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new JsonException($"The value '{value}' could not be converted to System.TimeSpan.");
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture));
    }
}
