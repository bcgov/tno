using System.Text.Json;
using System.Text.Json.Serialization;

namespace TNO.Core.Converters
{
    public class BooleanConverter : JsonConverter<bool>
    {
        #region Methods
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetInt32();
            return value != 0;
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value ? 1 : 0);
        }
        #endregion
    }
}
