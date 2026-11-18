using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TNO.Core.Extensions;

/// <summary>
/// JsonElementExtensions static class, provides extension methods for JsonElement objects.
/// </summary>
public static class JsonElementExtensions
{
    /// <summary>
    /// Serialize the specified 'doc'.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string ToJson(this JsonElement element, JsonWriterOptions? options = null)
    {
        options ??= new JsonWriterOptions { Indented = true };
        using var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream, options.Value);
        element.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Get the JsonElement for the specified 'path'.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static JsonElement? GetElement(this JsonElement element, string path)
    {
        JsonElement? node = element;
        if (node?.ValueKind == JsonValueKind.Null || node?.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        var segments = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            if (node == null || node?.ValueKind == JsonValueKind.Null || node?.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            if (int.TryParse(segment, out int index))
            {
                // A number is used to reference the specific position in the array.
                node = index >= 0 && node?.GetArrayLength() > index ? node?[index] : null;
            }
            else if (node?.ValueKind == JsonValueKind.Array)
            {
                return node;
            }
            else
            {
                node = node?.TryGetProperty(segment, out JsonElement value) == true ? value : null;
            }
        }

        return node;
    }

    /// <summary>
    /// Get the value for the specified 'path'.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <param name="path"></param>
    /// <param name="defaultValue"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static T? GetElementValue<T>(this JsonElement element, string path = "", T? defaultValue = default, JsonSerializerOptions? options = null)
    {
        var property = path.Contains('.') ? GetElement(element, path) : element;

        if (property == null) return defaultValue;
        var node = (JsonElement)property;
        var type = typeof(T);
        return node.ValueKind switch
        {
            JsonValueKind.String => ((Func<T?>)(() =>
            {
                var isNullableEnum = type.IsNullable() && Nullable.GetUnderlyingType(type)?.IsEnum == true;
                if (isNullableEnum)
                {
                    var eType = Nullable.GetUnderlyingType(type);
                    if (eType != null && Enum.TryParse(eType, node.GetString(), true, out var value))
                    {
                        return (T)value;
                    }
                    return defaultValue;
                }
                return type.IsEnum ? (T)Enum.Parse(type, node.GetString() ?? "") : (T)Convert.ChangeType($"{node.GetString()}".Trim(), type);
            }))(),
            JsonValueKind.Null or JsonValueKind.Undefined => defaultValue,
            JsonValueKind.Number => node.ConvertNumberTo<T>(),
            JsonValueKind.True or JsonValueKind.False => (T)(object)node.GetBoolean(),
            JsonValueKind.Object => node.Deserialize<T>(options),
            JsonValueKind.Array => node.Deserialize<T>(options),
            _ => (T)Convert.ChangeType($"{node}", type),
        };
    }

    /// <summary>
    /// Convert number to specified type.
    /// This method handles enum values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <returns></returns>
    public static T? ConvertNumberTo<T>(this JsonElement element)
    {
        var value = element.ToString() ?? "";
        var isFloat = Regex.IsMatch(value, "[-+]?[0-9]*\\.[0-9]+");
        if (isFloat)
        {
            if (element.TryGetSingle(out float fvalue)) return fvalue.ChangeType<T>();
            else if (element.TryGetDouble(out double dvalue)) return dvalue.ChangeType<T>();
        }
        else if (element.TryGetInt16(out short svalue))
        {
            var utype = Nullable.GetUnderlyingType(typeof(T));
            if (utype?.IsEnum == true && Enum.TryParse(utype, value, true, out object? evalue))
                return (T?)evalue;
            else if (typeof(T).IsEnum)
                return (T)(object)svalue;
            return svalue.ChangeType<T>();
        }
        else if (element.TryGetInt32(out int ivalue))
        {
            var utype = Nullable.GetUnderlyingType(typeof(T));
            if (utype?.IsEnum == true && Enum.TryParse(utype, value, true, out object? evalue))
                return (T?)evalue;
            else if (typeof(T).IsEnum)
                return (T)(object)ivalue;
            return ivalue.ChangeType<T>();
        }
        else if (element.TryGetInt64(out long lvalue))
        {
            var utype = Nullable.GetUnderlyingType(typeof(T));
            if (utype?.IsEnum == true && Enum.TryParse(utype, value, true, out object? evalue))
                return (T?)evalue;
            else if (typeof(T).IsEnum)
                return (T)(object)lvalue;
            return lvalue.ChangeType<T>();
        }
        return default;
    }
}
