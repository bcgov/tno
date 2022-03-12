
namespace TNO.Tools.Import.ETL;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.Tools.Import.ETL.Config;

/// <summary>
/// Program class, provides a console application to run an ETL service.
/// </summary>
class Program
{
    /// <summary>
    /// Initialies the static environment variables.
    /// </summary>
    static Program()
    {
        DotNetEnv.Env.Load();
    }

    /// <summary>
    /// Program main method.
    /// </summary>
    /// <param name="createdOn"></param>
    /// <returns></returns>
    static async Task<int> Main(DateTime? createdOn)
    {
        var envCreatedOn = Environment.GetEnvironmentVariable("CREATED_ON");
        var filterCreatedOn = createdOn ?? (!String.IsNullOrWhiteSpace(envCreatedOn) ? DateTime.Parse(envCreatedOn) : DateTime.Now.AddDays(-7));
        var config = Configure(new[] { filterCreatedOn.ToString() })
            .Build();

        var services = new ServiceCollection()
          .AddSingleton<IConfiguration>(config)
          .AddLogging(options =>
          {
              options.AddConfiguration(config.GetSection("Logging"));
              options.AddConsole();
          })
          .AddTransient<ImportService>();

        Exception? connectionError = null;
        try
        {
            services.AddSourceContext(config);
            services.AddDestinationContext(config);
        }
        catch (Exception ex)
        {
            connectionError = ex;
        }

        var provider = services.BuildServiceProvider();
        var logger = provider.GetService<ILogger<Program>>();
        var result = 0;

        try
        {
            if (connectionError != null) throw connectionError;
            var app = provider.GetService<ImportService>();
            if (app == null) throw new Exception("App ImportService not configured");

            result = await app.Run(filterCreatedOn);
        }
        catch (Exception ex)
        {
            logger?.LogCritical(ex, "An unhandled error has occurred.");
            result = 1;
        }

        provider.Dispose();
        return 0;
    }

    /// <summary>
    /// Configure host.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static IConfigurationBuilder Configure(string[] args)
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args ?? Array.Empty<String>());
    }
}