using Microsoft.Extensions.DependencyInjection;
using TNO.Services.FileMonitor.Config;
using TNO.Services.Runners;

namespace TNO.Services.FileMonitor;

/// <summary>
/// SyndicationService class, provides a console application that runs service, and an api.
/// </summary>
public class FileMonitorService : IngestService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public FileMonitorService(string[] args) : base(args)
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
            .Configure<FileMonitorOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IIngestAction<FileMonitorOptions>, FileMonitorAction>()
            .AddTransient<IngestManagerFactory<FileMonitorIngestActionManager, FileMonitorOptions>>()
            .AddSingleton<IServiceManager, FileMonitorManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<SyndicationOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
