using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TNO.CSS.Models;

namespace TNO.CSS.Converters;

public class UserRoleOperationConverter : JsonConverter<UserRoleOperation>
{
    public override UserRoleOperation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var operation = reader.GetString();
        return operation switch
        {
            "add" => UserRoleOperation.Add,
            "del" => UserRoleOperation.Delete,
            _ => throw new FormatException("Operation not a correct value."),
        };
    }

    public override void Write(Utf8JsonWriter writer, UserRoleOperation value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
