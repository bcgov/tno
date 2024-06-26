using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.Services.ContentMigration.Config;

namespace TNO.Services.ContentMigration.Sources.Oracle.Services;

/// <summary>
/// ServiceCollectionExtensions static class, provides extensions methods for IServiceCollection objects.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add a Oracle DbContext to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddMigrationSourceContext(this IServiceCollection services, OracleConnectionSettings? config)
    {
        string connectionString = string.Empty;
        if (config != null)
        {
            connectionString = OracleConnectionStringHelper.GetConnectionString(config.UserName, config.Password, config.HostName, config.Port, config.Sid);
        }

        services.AddDbContext<MigrationSourceContext>(options =>
        {
            DbContextOptionsBuilder db;
            if (connectionString != null)
            {
                db = options.UseOracle(connectionString, options =>
                {
                    options.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds);
                });
            }
            else
            {
                db = options.UseOracle(options =>
                {
                    options.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds);
                });
            }

            var debugLoggerFactory = LoggerFactory.Create(builder => { builder.AddDebug(); }); // NOSONAR
            db.UseLoggerFactory(debugLoggerFactory);
            options.EnableSensitiveDataLogging();
        });

        return services;
    }
}
