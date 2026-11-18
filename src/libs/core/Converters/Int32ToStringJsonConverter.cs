using System.Text.Json;
using System.Text.Json.Serialization;

namespace TNO.Core.Converters
{
    public class Int32ToStringJsonConverter : JsonConverter<string>
    {
        #region Methods
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.TryGetInt32(out int result) ? $"{result}" : "",
                _ => null,
            };

            if (value != null) return value;

            var startDepth = reader.CurrentDepth;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth) break;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
        #endregion
    }
}
