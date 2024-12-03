using MovieWpfApp.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovieWpfApp.Converters
{
    internal sealed class MovieModelBaseConverter : JsonConverter<MovieModelBase>
    {
        public override MovieModelBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var rootElement = jsonDoc.RootElement;

            var kind = (MovieKind)Enum.Parse(typeof(MovieKind), rootElement.GetProperty("Kind").GetString()!);
            var rawText = rootElement.GetRawText();
            switch (kind)
            {
                case MovieKind.Group:
                    var movieGroup = JsonSerializer.Deserialize<MovieGroupModel>(rawText, options);
                    SetParentForNestedItems(movieGroup!.Items, movieGroup);
                    return movieGroup;
                case MovieKind.Movie:
                    var movieModel = JsonSerializer.Deserialize<MovieModel>(rawText, options)!;
                    return movieModel;
            }

            throw new NotImplementedException();
        }

        private static void SetParentForNestedItems(ObservableCollection<MovieModelBase> items, MovieGroupModel parent)
        {
            foreach (var item in items)
            {
                item.Parent = parent;
                if (item is MovieGroupModel group)
                {
                    SetParentForNestedItems(group!.Items, group);
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, MovieModelBase value, JsonSerializerOptions options)
        {
            if (value is MovieGroupModel group)
            {
                JsonSerializer.Serialize(writer, group, options);
            }
            else if (value is MovieModel movie)
            {
                JsonSerializer.Serialize(writer, movie, options);
            }
            else
            {
                throw new JsonException($"Unknown type when writing {nameof(MovieModelBase)}");
            }
        }
    }
}
