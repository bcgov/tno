using Microsoft.Extensions.DependencyInjection;
using RazorEngineCore;

namespace TNO.TemplateEngine;

/// <summary>
/// ServiceCollectionExtensions static class, provides extension methods for IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the template engine to dependency injection service provider.
    /// </summary>
    /// <typeparam name="T">Template model type.</typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddTemplateEngine<T>(this IServiceCollection services)
        where T : IRazorEngineTemplate
    {
        return services
            .AddScoped<IRazorEngine, RazorEngine>()
            .AddScoped<ITemplateEngine<T>, TemplateEngine<T>>();
    }
}
