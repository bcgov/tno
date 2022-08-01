using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Runners;
using TNO.Services.Filemonitor.Config;

namespace TNO.Services.Filemonitor;

/// <summary>
/// SyndicationService abstrct class, provides a console application that runs service, and an api.
/// </summary>
public class FilemonitorService : IngestService
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
    public FilemonitorService(string[] args) : base(args)
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
            .Configure<FilemonitorOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IIngestAction<FilemonitorOptions>, FilemonitorAction>()
            .AddTransient<DataSourceIngestManagerFactory<FilemonitorDataSourceManager, FilemonitorOptions>>()
            .AddSingleton<IServiceManager, FilemonitorManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<SyndicationOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
