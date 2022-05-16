using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace TNO.Services.Runners;

/// <summary>
/// BaseService abstract class, provides a console application that runs a service, and an api.
/// The main purpose of the KafkaProducerService is to configure Kafka producer dependency injection.
/// </summary>
public abstract class KafkaProducerService : BaseService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KafkaProducerService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public KafkaProducerService(string[] args) : base(args)
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
            .Configure<ProducerConfig>(this.Configuration.GetSection("Kafka:Producer"))
            .AddTransient<IKafkaMessenger, KafkaMessenger>();

        return services;
    }
    #endregion
}
