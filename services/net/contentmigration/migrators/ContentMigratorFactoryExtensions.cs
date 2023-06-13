using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TNO.Services.ContentMigration.Config;

namespace TNO.Services.ContentMigration.Migrators;

/// <summary>
///
/// </summary>
public static class ContentMigratorFactoryExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddSourceMigrators(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<ClipMigrationOptions>(configuration.GetSection("Service:ClipMigrator"));
        services.AddScoped<ClipMigrator>()
                        .AddScoped<IContentMigrator, ClipMigrator>(s => s.GetService<ClipMigrator>());

        services.Configure<ImageMigrationOptions>(configuration.GetSection("Service:ImageMigrator"));
        services.AddScoped<ImageMigrator>()
                        .AddScoped<IContentMigrator, ImageMigrator>(s => s.GetService<ImageMigrator>());

        services.Configure<PaperMigrationOptions>(configuration.GetSection("Service:PaperMigrator"));
        services.AddScoped<PaperMigrator>()
                        .AddScoped<IContentMigrator, PaperMigrator>(s => s.GetService<PaperMigrator>());

        services.AddScoped<ContentMigratorFactory>();

        return services;
    }

}
