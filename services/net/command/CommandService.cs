using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Command.Config;
using TNO.Services.Runners;

namespace TNO.Services.Command;

/// <summary>
/// CommandService abstrct class, provides a console application that runs service, and an api.
/// </summary>
public class CommandService : IngestService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public CommandService(string[] args) : base(args)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Configure dependency injection.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services
            .Configure<CommandOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IIngestAction<CommandOptions>, CommandAction>()
            .AddTransient<IngestManagerFactory<CommandIngestActionManager, CommandOptions>>()
            .AddSingleton<IServiceManager, CommandManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<CommandOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
