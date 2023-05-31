using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

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
    /// <param name="connectionString"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IServiceCollection AddMigrationSourceContext(this IServiceCollection services, string connectionString)
    {
        if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Argument is required and cannot be null, empty or whitespace.", nameof(connectionString));

        services.AddDbContext<MigrationSourceContext>(options =>
        {
            var db = options.UseOracle(connectionString, options =>
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
    /// Add a Oracle DbContext to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddMigrationSourceContext(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config["DB_ORACLE_CS"];
        var userId = config["DB_ORACLE_USERNAME"];
        var pwd = config["DB_ORACLE_PASSWORD"];
        var oracleBuilder = new OracleConnectionStringBuilder(connectionString);
        oracleBuilder.UserID = userId;
        oracleBuilder.Password = pwd;

        return services.AddMigrationSourceContext(oracleBuilder.ConnectionString);
    }
}
