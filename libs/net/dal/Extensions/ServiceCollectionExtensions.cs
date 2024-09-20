using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Elastic;
using Amazon.S3;

namespace TNO.DAL;

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
    public static IServiceCollection AddTNOContext(this IServiceCollection services, string connectionString, IHostEnvironment env)
    {
        if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Argument is required and cannot be null, empty or whitespace.", nameof(connectionString));

        services.AddDbContext<TNOContext>(options =>
        {
            var db = options.UseNpgsql(connectionString, options =>
            {
                options.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds);
            });
            options.EnableSensitiveDataLogging(env.IsDevelopment());
            options.EnableDetailedErrors(env.IsDevelopment());
            if (env.IsDevelopment())
            {
                var debugLoggerFactory = LoggerFactory
                    .Create(builder =>
                    {
                        builder
                            .AddConsole()
                            .AddDebug();
                    });
                db.UseLoggerFactory(debugLoggerFactory);
            }
        });

        return services;
    }

    /// <summary>
    /// Add a PostgreSQL DbContext to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="env"></param>
    /// <returns></returns>
    public static IServiceCollection AddTNOContext(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var postgresBuilder = new NpgsqlConnectionStringBuilder(config["ConnectionStrings:TNO"])
        {
            Username = config["DB_POSTGRES_USERNAME"],
            Password = config["DB_POSTGRES_PASSWORD"]
        };
        return services.AddTNOContext(postgresBuilder.ConnectionString, env);
    }

    /// <summary>
    /// Add a PostgreSQL DbContext to the service collection and all the related services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="env"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddTNOServices(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        services.AddTNOContext(config, env)
            .AddStorageConfig(config)
            .AddElastic(config, env)
            .AddScoped<IElasticsearchService, ElasticsearchService>();

        // Find all the configuration classes.
        var assembly = typeof(BaseService).Assembly;
        var type = typeof(IBaseService);
        var tnoServiceTypes = assembly.GetTypes().Where(t => !t.IsAbstract && t.IsClass && t.GetInterfaces().Any(i => i.Name.Equals(type.Name)));
        foreach (var serviceType in tnoServiceTypes)
        {
            var sinterface = serviceType.GetInterface($"I{serviceType.Name}") ?? throw new InvalidOperationException($"Service type '{serviceType.Name}' is missing its interface.");
            services.AddScoped(sinterface, serviceType);
        }

        return services;
    }

    /// <summary>
    /// Add the storage configuration to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddStorageConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<StorageOptions>(config.GetSection("Storage"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<StorageOptions>>().Value);
        services.AddOptions<StorageOptions>()
            .Bind(config.GetSection("Storage"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

}
