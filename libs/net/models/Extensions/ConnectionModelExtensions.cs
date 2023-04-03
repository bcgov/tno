using System.Text.Json;
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
    /// <param name="configuration"></param>
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
                JsonValueKind.Number => element.GetInt64().ChangeType<T>(), // TODO: Handle different number types.
                JsonValueKind.True or JsonValueKind.False => (T)Convert.ChangeType($"{element.GetBoolean()}", typeof(T)),
                _ => (T)Convert.ChangeType($"{element}", typeof(T)),
            };
        }

        throw new ConfigurationException($"Dictionary key '{key}' is not a valid JSON element");
    }
}
