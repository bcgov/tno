using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace TNO.Services.Runners;

/// <summary>
/// BaseService abstract class, provides a console application that runs a service, and an api.
/// The main purpose of the BaseService is to configure Kafka consumer dependency injection.
/// </summary>
public abstract class KafkaConsumerService : BaseService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KafkaConsumerService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public KafkaConsumerService(string[] args) : base(args)
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
            .Configure<ConsumerConfig>(this.Configuration.GetSection("Kafka:Consumer"))
            .AddTransient<IKafkaListener, KafkaListener>();

        return services;
    }
    #endregion
}
