using System.Reflection;
using System.Text.Json;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;

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
            if (typeof(T).GetTypeInfo().IsEnum && element.ValueKind.Equals(JsonValueKind.String))
                return (T)Enum.Parse(typeof(T), $"{element.GetString()}".Trim());
            else
                return ConvertJsonValue<T>(element);
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
            if (typeof(T).GetTypeInfo().IsEnum && element.ValueKind.Equals(JsonValueKind.String))
            {
                if (!Enum.IsDefined(typeof(T), $"{element.GetString()}".Trim()))
                    return defaultValue;

                return (T)Enum.Parse(typeof(T), $"{element.GetString()}".Trim());
            }
            else
            {
                return ConvertJsonValue<T>(element) ?? defaultValue;
            }
        }

        throw new ConfigurationException($"Ingest configuration '{key}' is not a valid JSON element");
    }

    /// <summary>
    /// Convert the JSON value to the correct type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static T? ConvertJsonValue<T>(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => (T)Convert.ChangeType($"{element.GetString()}".Trim(), typeof(T)),
            JsonValueKind.Null or JsonValueKind.Undefined => default,
            JsonValueKind.Number => Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Int32 => typeof(T).IsNullable()
                    ? (int.TryParse($"{element.GetInt32()}", out int result) ? (T)(object)result : default)
                    : (T)Convert.ChangeType($"{element.GetInt32()}", typeof(T)),
                TypeCode.Int64 => typeof(T).IsNullable()
                    ? (long.TryParse($"{element.GetInt64()}", out long result) ? (T)(object)result : default)
                    : (T)Convert.ChangeType($"{element.GetInt64()}", typeof(T)),
                TypeCode.Decimal => typeof(T).IsNullable()
                    ? (decimal.TryParse($"{element.GetDecimal()}", out decimal result) ? (T)(object)result : default)
                    : (T)Convert.ChangeType($"{element.GetDecimal()}", typeof(T)),
                TypeCode.Double => typeof(T).IsNullable()
                    ? (double.TryParse($"{element.GetDouble()}", out double result) ? (T)(object)result : default)
                    : (T)Convert.ChangeType($"{element.GetDouble()}", typeof(T)),
                TypeCode.Single => typeof(T).IsNullable()
                    ? (float.TryParse($"{element.GetSingle()}", out float result) ? (T)(object)result : default)
                    : (T)Convert.ChangeType($"{element.GetSingle()}", typeof(T)),
                TypeCode.DateTime => typeof(T).IsNullable()
                    ? (DateTime.TryParse($"{element.GetDateTime()}", out DateTime result) ? (T)(object)result : default)
                    : (T)Convert.ChangeType($"{element.GetDateTime()}", typeof(T)),
                TypeCode.Boolean => typeof(T).IsNullable()
                    ? (bool.TryParse($"{element.GetBoolean()}", out bool result) ? (T)(object)result : default)
                    : (T)Convert.ChangeType($"{element.GetBoolean()}", typeof(T)),
                _ => !typeof(T).IsNullable() // Handle nullable types.
                    ? (T)Convert.ChangeType($"{element.GetRawText()}", typeof(T))
                    : Type.GetTypeCode(Nullable.GetUnderlyingType(typeof(T))) switch
                    {
                        TypeCode.Int32 => int.TryParse($"{element.GetInt32()}", out int nullResult) ? (T)(object)nullResult : default,
                        TypeCode.Int64 => long.TryParse($"{element.GetInt64()}", out long nullResult) ? (T)(object)nullResult : default,
                        TypeCode.Decimal => decimal.TryParse($"{element.GetDecimal()}", out decimal nullResult) ? (T)(object)nullResult : default,
                        TypeCode.Double => double.TryParse($"{element.GetDouble()}", out double nullResult) ? (T)(object)nullResult : default,
                        TypeCode.Single => float.TryParse($"{element.GetSingle()}", out float nullResult) ? (T)(object)nullResult : default,
                        TypeCode.DateTime => DateTime.TryParse($"{element.GetDateTime()}", out DateTime nullResult) ? (T)(object)nullResult : default,
                        TypeCode.Boolean => bool.TryParse($"{element.GetBoolean()}", out bool nullResult) ? (T)(object)nullResult : default,
                        _ => (T)Convert.ChangeType($"{element.GetRawText()}", typeof(T)),
                    }
            },
            JsonValueKind.True or JsonValueKind.False => (T)Convert.ChangeType($"{element.GetBoolean()}", typeof(T)),
            JsonValueKind.Object => (T)Convert.ChangeType($"{element.GetRawText()}", typeof(T)),
            JsonValueKind.Array => (T)Convert.ChangeType($"{element.GetRawText()}", typeof(T)),
            _ => (T)Convert.ChangeType($"{element}", typeof(T)),
        };
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
