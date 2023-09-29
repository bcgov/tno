using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Config;

namespace TNO.Services.Runners;

/// <summary>
/// BaseService abstract class, provides a console application that runs a service, and an api.
/// The main purpose of the IngestService is to configure ingest dependency injection.
/// </summary>
public abstract class IngestService : BaseService
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public IngestService(string[] args) : base(args)
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
            .Configure<IngestServiceOptions>(this.Configuration.GetSection("Service"));

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<IngestServiceOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
