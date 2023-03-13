using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TNO.Core.Extensions;
using TNO.DAL;

namespace TNO.Elastic.Migration;

/// <summary>
/// The TNO Elasticsearch Migration tool provides a way to perform schema migrations and database changes.
///
/// - Re-index content so that it has the latest shape
/// - Run schema migrations to Elasticsearch
/// </summary>
class Program
{
    #region Variables
    private static Dictionary<string, string> _argMapping = new()
        {
            { "-v", "Elastic__MigrationVersion" },
            { "--version", "Elastic__MigrationVersion" },
        };
    #endregion

    #region Methods
    /// <summary>
    /// Initialize and run the service.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static async Task Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();
        var tokenSource = new CancellationTokenSource();
        var service = host.Services.GetRequiredService<MigrationService>();
        await service.RunAsync(tokenSource.Token);
    }

    /// <summary>
    /// Setup the program host that will run the services.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        DotNetEnv.Env.Load();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return Host.CreateDefaultBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseConsoleLifetime()
            .ConfigureHostConfiguration(config =>
            {
                config
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args, _argMapping);
            })
            .ConfigureLogging((context, config) =>
            {
                config.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services
                    .AddLogging()
                    .AddSerializerOptions(context.Configuration)
                    .Configure<ElasticMigrationOptions>(context.Configuration.GetSection("Elastic"))
                    .AddTNOServices(context.Configuration, context.HostingEnvironment)
                    .AddElasticMigration(context.Configuration)
                    .AddSingleton<ElasticsearchClient, TNOElasticClient>()
                    .AddSingleton<MigrationService>();
            });
    }
    #endregion
}
