using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using TNO.DAL;
using TNO.Tools.Import.Source;

namespace TNO.Tools.Import.ETL.Config;

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
    public static IServiceCollection AddSourceContext(this IServiceCollection services, string connectionString)
    {
        if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Argument is required and cannot be null, empty or whitespace.", nameof(connectionString));

        services.AddDbContext<SourceContext>(options =>
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
    public static IServiceCollection AddSourceContext(this IServiceCollection services, IConfiguration config)
    {
        var oracleBuilder = new OracleConnectionStringBuilder(config["DB_ORACLE_CS"]);
        var userId = config["DB_ORACLE_USERNAME"];
        var pwd = config["DB_ORACLE_PASSWORD"];
        oracleBuilder.UserID = userId;
        oracleBuilder.Password = pwd;
        return services.AddSourceContext(oracleBuilder.ConnectionString);
    }

    /// <summary>
    /// Add a PostgreSQL DbContext to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IServiceCollection AddDestinationContext(this IServiceCollection services, string connectionString)
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
    public static IServiceCollection AddDestinationContext(this IServiceCollection services, IConfiguration config)
    {
        var postgresBuilder = new NpgsqlConnectionStringBuilder(config["DB_POSTGRES_CS"]);
        var userId = config["DB_POSTGRES_USERNAME"];
        var pwd = config["DB_POSTGRES_PASSWORD"];
        postgresBuilder.Username = userId;
        postgresBuilder.Password = pwd;
        return services.AddDestinationContext(postgresBuilder.ConnectionString);
    }
}