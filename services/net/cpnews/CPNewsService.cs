using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Runners;
using TNO.Services.CPNews.Config;

namespace TNO.Services.CPNews;

/// <summary>
/// CPNewsService abstrct class, provides a console application that runs service, and an api.
/// </summary>
public class CPNewsService : IngestService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CPNewsService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public CPNewsService(string[] args) : base(args)
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
            .Configure<CPNewsOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IIngestAction<CPNewsOptions>, CPNewsAction>()
            .AddTransient<DataSourceIngestManagerFactory<CPNewsDataSourceManager, CPNewsOptions>>()
            .AddSingleton<IServiceManager, CPNewsManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<CPNewsOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
