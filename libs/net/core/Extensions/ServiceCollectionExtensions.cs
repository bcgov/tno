using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TNO.Core.Converters;

namespace TNO.Core.Extensions;

/// <summary>
/// ServiceCollectionExtensions static class, provides extension methods for ServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static JsonSerializerOptions GetSerializerOptions(this IConfiguration config)
    {
        var defaultIgnoreCondition = config.GetValue<JsonIgnoreCondition>("Serialization:Json:DefaultIgnoreCondition", JsonIgnoreCondition.WhenWritingNull);
        var propertyNameCaseInsensitive = config.GetValue<bool>("Serialization:Json:PropertyNameCaseInsensitive", false);
        var propertyNamingPolicy = config.GetValue<string>("Serialization:Json:PropertyNamingPolicy");
        var writeIndented = config.GetValue<bool>("Serialization:Json:WriteIndented", false);
        return new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = defaultIgnoreCondition,
            PropertyNameCaseInsensitive = propertyNameCaseInsensitive,
            PropertyNamingPolicy = propertyNamingPolicy == "CamelCase" ? JsonNamingPolicy.CamelCase : null,
            WriteIndented = writeIndented
        };
    }

    /// <summary>
    /// Add the serialization options from configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddSerializerOptions(this IServiceCollection services, IConfiguration config)
    {
        var jsonSerializerOptions = config.GetSerializerOptions();
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.DefaultIgnoreCondition = jsonSerializerOptions.DefaultIgnoreCondition;
            options.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
            options.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
            options.WriteIndented = jsonSerializerOptions.WriteIndented;
            options.Converters.Add(new JsonStringEnumConverter());
            // options.Converters.Add(new Int32ToStringJsonConverter());
        });

        return services;
    }
}
