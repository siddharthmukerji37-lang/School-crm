using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolCRM.API.Converters;

public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return default;

        if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        throw new JsonException($"The value '{value}' could not be converted to System.TimeOnly.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
    }
}
