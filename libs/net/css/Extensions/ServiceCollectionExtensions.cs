using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TNO.Core.Http;
using TNO.CSS.Config;

namespace TNO.CSS;

/// <summary>
/// ServiceCollectionExtensions static class, provides extension methods for ServiceCollectionExtensions objects.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the CssService to the dependency injection service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddCssService(this IServiceCollection services, IConfiguration config)
    {
        return services
            .Configure<CssOptions>(config)
            .AddScoped<IHttpRequestClient, HttpRequestClient>()
            .AddScoped<ICssClient, CssClient>()
            .AddScoped<ICssService, CssService>();
    }

    /// <summary>
    /// Add the CssEnvironmentService to the dependency injection service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddCssEnvironmentService(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddCssService(config)
            .Configure<CssEnvironmentOptions>(config)
            .AddScoped<ICssEnvironmentService, CssEnvironmentService>();
    }
}
