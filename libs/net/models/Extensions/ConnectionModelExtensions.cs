using System.Text.Json;
using System.Text.RegularExpressions;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;

namespace TNO.Models.Extensions;

/// <summary>
/// ConnectionModelExtensions static class, provides extension methods for ConnectionModel.
/// </summary>
public static class ConnectionModelExtensions
{
    /// <summary>
    /// Get the connection setting value for the specified key.
    /// Or return an empty string if there is no key or value.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetConfigurationValue(this ConnectionModel model, string key)
    {
        return GetConfigurationValue<string>(model, key) ?? "";
    }

    /// <summary>
    /// Get the connection setting value for the specified key.
    /// Or return an empty string if there is no key or value.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T? GetConfigurationValue<T>(this ConnectionModel model, string key)
    {
        return model.Configuration.GetDictionaryJsonValue<T>(key);
    }

    /// <summary>
    /// Get the connection setting value for the specified key.
    /// Or return an empty string if there is no key or value.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetConfigurationValue(this API.Areas.Services.Models.DataLocation.ConnectionModel model, string key)
    {
        return GetConfigurationValue<string>(model, key) ?? "";
    }

    /// <summary>
    /// Get the connection setting value for the specified key.
    /// Or return an empty string if there is no key or value.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T? GetConfigurationValue<T>(this API.Areas.Services.Models.DataLocation.ConnectionModel model, string key)
    {
        return model.Configuration.GetDictionaryJsonValue<T>(key);
    }

    /// <summary>
    /// Get the dictionary value for the specified key.
    /// If the value is a JsonElement then convert it to the specified 'T' type.
    /// If the key does not exist it will return the 'default' value for the specified 'T' type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configurtion"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public static T? GetDictionaryJsonValue<T>(this Dictionary<string, object> configuration, string key)
    {
        if (!configuration.TryGetValue(key, out object? value)) return default;

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => (T)Convert.ChangeType($"{element.GetString()}".Trim(), typeof(T)),
                JsonValueKind.Null or JsonValueKind.Undefined => default,
                JsonValueKind.Number => ConvertNumberTo<T>(element),
                JsonValueKind.True or JsonValueKind.False => (T)Convert.ChangeType($"{element.GetBoolean()}", typeof(T)),
                _ => (T)Convert.ChangeType($"{element}", typeof(T)),
            };
        }

        throw new ConfigurationException($"Dictionary key '{key}' is not a valid JSON element");
    }

    /// <summary>
    /// Convert number to specified type.
    /// This method handles enum values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <returns></returns>
    private static T? ConvertNumberTo<T>(JsonElement element)
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
