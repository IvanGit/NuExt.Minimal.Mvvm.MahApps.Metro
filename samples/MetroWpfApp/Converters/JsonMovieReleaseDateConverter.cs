using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetroWpfApp.Converters
{
    public sealed class JsonMovieReleaseDateConverter: JsonConverter<DateTime>
    {
        private const string DateFormat = "yyyy.MM.dd";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            var dateString = reader.GetString();
            if (DateTime.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out var date))
            {
                return date;
            }

            throw new JsonException($"Unable to convert \"{dateString}\" to DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateFormat));
        }
    }
}
