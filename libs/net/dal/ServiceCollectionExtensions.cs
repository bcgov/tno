using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using TNO.DAL.Services;

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
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IServiceCollection AddTNOContext(this IServiceCollection services, string connectionString)
    {
        if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Argument is required and cannot be null, empty or whitespace.", nameof(connectionString));

        services.AddDbContext<TNOContext>(options =>
        {
            var db = options.UseNpgsql(connectionString, options =>
            {
                options.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds);
            });
            var debugLoggerFactory = LoggerFactory.Create(builder => { builder.AddDebug(); }); // NOSONAR
            db.UseLoggerFactory(debugLoggerFactory);
            options.EnableSensitiveDataLogging();
        });

        return services;
    }

    /// <summary>
    /// Add a PostgreSQL DbContext to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddTNOContext(this IServiceCollection services, IConfiguration config)
    {
        var postgresBuilder = new NpgsqlConnectionStringBuilder(config["ConnectionStrings:TNO"])
        {
            Username = config["DB_POSTGRES_USERNAME"],
            Password = config["DB_POSTGRES_PASSWORD"]
        };
        return services.AddTNOContext(postgresBuilder.ConnectionString);
    }

    /// <summary>
    /// Add a PostgreSQL DbContext to the service collection and all the services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddTNOServices(this IServiceCollection services, IConfiguration config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        services.AddTNOContext(config);

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
}
