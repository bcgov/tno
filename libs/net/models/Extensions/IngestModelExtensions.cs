using System.Reflection;
using System.Text.Json;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Exceptions;

namespace TNO.Models.Extensions;

/// <summary>
/// IngestModelExtensions static class, provides extension methods for IngestModel.
/// </summary>
public static class IngestModelExtensions
{
    /// <summary>
    /// Get the configuration setting value for the specified key.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException">If the key or value are invalid.</exception>
    public static string GetConfigurationValue(this IngestModel ingest, string key)
    {
        return GetConfigurationValue<string>(ingest, key) ?? "";
    }

    /// <summary>
    /// Get the configuration setting value for the specified key.
    /// Or return the default value if the key does not exist.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException">If the key or value are invalid.</exception>
    public static string GetConfigurationValue(this IngestModel ingest, string key, string defaultValue)
    {
        return GetConfigurationValue<string>(ingest, key, defaultValue);
    }

    /// <summary>
    /// Get the configuration setting value for the specified key.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException">If the key or value are invalid.</exception>
    public static T? GetConfigurationValue<T>(this IngestModel ingest, string key)
    {
        if (!ingest.Configuration.TryGetValue(key, out object? value)) return default;

        if (value is JsonElement element)
        {
            if (typeof(T).GetTypeInfo().IsEnum)
                return (T)Enum.Parse(typeof(T), $"{element.GetString()}".Trim());
            else
                return element.ValueKind switch
                {
                    JsonValueKind.String => (T)Convert.ChangeType($"{element.GetString()}".Trim(), typeof(T)),
                    JsonValueKind.Null or JsonValueKind.Undefined => default,
                    JsonValueKind.Number => (T)Convert.ChangeType($"{element.GetInt32()}", typeof(T)),
                    JsonValueKind.True or JsonValueKind.False => (T)Convert.ChangeType($"{element.GetBoolean()}", typeof(T)),
                    _ => (T)Convert.ChangeType($"{element}", typeof(T)),
                };
        }

        throw new ConfigurationException($"Ingest configuration '{key}' is not a valid JSON element");
    }

    /// <summary>
    /// Get the configuration setting value for the specified key.
    /// Or return the default value if the key does not exist.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException">If the key or value are invalid.</exception>
    public static T GetConfigurationValue<T>(this IngestModel ingest, string key, T defaultValue)
    {
        if (!ingest.Configuration.TryGetValue(key, out object? value)) return defaultValue;

        if (value is JsonElement element)
        {
            if (typeof(T).GetTypeInfo().IsEnum)
            {
                if (!Enum.IsDefined(typeof(T), $"{element.GetString()}".Trim()))
                    return defaultValue;

                return (T)Enum.Parse(typeof(T), $"{element.GetString()}".Trim());
            }
            else
            {
                return (element.ValueKind switch
                {
                    JsonValueKind.String => (T)Convert.ChangeType($"{element.GetString()}".Trim(), typeof(T)),
                    JsonValueKind.Null or JsonValueKind.Undefined => default,
                    JsonValueKind.Number => (T)Convert.ChangeType($"{element.GetInt32()}", typeof(T)),
                    JsonValueKind.True or JsonValueKind.False => (T)Convert.ChangeType($"{element.GetBoolean()}", typeof(T)),
                    _ => (T)Convert.ChangeType($"{element}", typeof(T)),
                }) ?? defaultValue;
            }
        }

        throw new ConfigurationException($"Ingest configuration '{key}' is not a valid JSON element");
    }

    /// <summary>
    /// Determine if the ingestion configuration should send content to Kafka.
    /// This looks for the `Ingest.Configuration.post=true` property.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public static bool PostToKafka(this IngestModel ingest)
    {
        return ingest.GetConfigurationValue("post", false);
    }

    /// <summary>
    /// Determine if the ingestion configuration content should be imported.
    /// This looks for the `Ingest.Configuration.import=true` property.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public static bool ImportContent(this IngestModel ingest)
    {
        return ingest.GetConfigurationValue("import", false);
    }
}
