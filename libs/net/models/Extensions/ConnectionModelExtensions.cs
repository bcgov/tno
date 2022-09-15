using System.Text.Json;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Exceptions;

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
    /// <param name="dataSource"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetConfigurationValue(this ConnectionModel dataSource, string key)
    {
        return GetConfigurationValue<string>(dataSource, key) ?? "";
    }
    /// <summary>
    /// Get the connection setting value for the specified key.
    /// Or return an empty string if there is no key or value.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T? GetConfigurationValue<T>(this ConnectionModel dataSource, string key)
    {
        if (!dataSource.Configuration.TryGetValue(key, out object? value)) return default;

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => (T)Convert.ChangeType($"{element.GetString()}".Trim(), typeof(T)),
                JsonValueKind.Null or JsonValueKind.Undefined => default,
                JsonValueKind.Number => (T)Convert.ChangeType($"{element.GetInt32()}", typeof(T)),
                JsonValueKind.True or JsonValueKind.False => (T)Convert.ChangeType($"{element.GetBoolean()}", typeof(T)),
                _ => (T)Convert.ChangeType($"{element}", typeof(T)),
            };
        }

        throw new ConfigurationException($"Connection configuration '{key}' is not a valid JSON element");
    }
}
