using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TNO.Elastic.Migration;

/// <summary>
/// ServiceCollectionExtensions static class, provide extension methods for service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the require elastic migration services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddElasticMigration(this IServiceCollection services, IConfiguration configure)
    {
        foreach (var type in typeof(Migration).Assembly.GetMigrationTypes())
        {
            services.AddScoped(type);
        }

        return services
            .Configure<ElasticMigrationOptions>(configure.GetSection("Elastic"))
            .AddSingleton(new ClaimsPrincipal())
            .AddScoped<MigrationBuilder>();
    }
}
