using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using TNO.Core.Extensions;

namespace TNO.DAL;

/// <summary>
/// TNOContextFactory class, provides a way to use the design time tools (i.e. dotnet ef database update).
/// </summary>
[ExcludeFromCodeCoverage]
public class TNOContextFactory : IDesignTimeDbContextFactory<TNOContext>
{
    #region Variables
    private readonly ILogger<TNOContextFactory> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TNOContextFactory class.
    /// </summary>
    public TNOContextFactory()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("TNO", LogLevel.Debug)
                .AddConsole();
        });
        _logger = loggerFactory.CreateLogger<TNOContextFactory>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Create the database context so that the design time tools can connect to it.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public TNOContext CreateDbContext(string[] args)
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        // As per Microsoft documentation, a typical sequence of configuration providers is:
        //   1. appsettings.json
        //   2. appsettings.{Environment}.json
        //   3. Secret Manager (if in development)
        //   4. Environment variables using the Environment Variables configuration provider.
        //   5. Command - line arguments using the Command-line configuration provider.
        // source: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#configuration-providers
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddDotNetEnv($"{Environment.CurrentDirectory}/.env")
            .AddJsonFile("connectionstrings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"connectionstrings.{environment}.json", optional: true, reloadOnChange: true);

        if (!environment.IsProduction())
        {
            builder.AddUserSecrets<TNOContext>();
        }

        builder.AddEnvironmentVariables();

        _logger.LogInformation("Context Factory Started");

        var config = builder.Build();
        var cs = config.GetConnectionString("TNO");
        var sqlBuilder = new NpgsqlConnectionStringBuilder(cs)
        {
            Username = config["DB_POSTGRES_USERNAME"],
            Password = config["DB_POSTGRES_PASSWORD"]
        };

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(sqlBuilder.ConnectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<TNOContext>();
        optionsBuilder.UseNpgsql(dataSource, options =>
        {
            options.CommandTimeout((int)TimeSpan.FromMinutes(60).TotalSeconds);
        });

        var serializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        var optionsSerializer = Microsoft.Extensions.Options.Options.Create(serializerOptions);
        return new TNOContext(optionsBuilder.Options, null, optionsSerializer);
    }
    #endregion
}
