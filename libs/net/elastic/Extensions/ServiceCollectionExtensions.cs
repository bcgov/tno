using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using TNO.Core.Http;

namespace TNO.Elastic;

/// <summary>
/// ServiceCollectionExtensions static class, provides extensions methods for IServiceCollection objects.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add a PostgreSQL DbContext to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <param name="env"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IServiceCollection AddElastic(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var options = config.GetSection("Elastic");
        services
            .AddScoped<IHttpRequestClient, HttpRequestClient>()
            .AddScoped<IElasticClient, TNOElasticClient>()
            .AddScoped<ITNOElasticClient, TNOElasticClient>()
            .Configure<ElasticOptions>(options)
            .AddOptions<ElasticOptions>()
                .Bind(options)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Add a PostgreSQL DbContext to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <param name="env"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IServiceCollection AddElasticSingleton(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var options = config.GetSection("Elastic");
        services
            .AddSingleton<IHttpRequestClient, HttpRequestClient>()
            .AddSingleton<IElasticClient, TNOElasticClient>()
            .AddSingleton<ITNOElasticClient, TNOElasticClient>()
            .Configure<ElasticOptions>(options)
            .AddOptions<ElasticOptions>()
                .Bind(options)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        return services;
    }
}
