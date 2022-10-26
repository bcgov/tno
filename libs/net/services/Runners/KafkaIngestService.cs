using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Config;

namespace TNO.Services.Runners;

/// <summary>
/// BaseService abstract class, provides a console application that runs a service, and an api.
/// The main purpose of the KafkaIngestService is to configure ingest dependency injection.
/// </summary>
public abstract class KafkaIngestService : KafkaProducerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KafkaIngestService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public KafkaIngestService(string[] args) : base(args)
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
        // services.AddOptions<KafkaIngestServiceOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
