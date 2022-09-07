using Microsoft.Extensions.DependencyInjection;
using Confluent.Kafka;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Indexing.Config;
using TNO.Services.Runners;

namespace TNO.Services.Indexing;

/// <summary>
/// IndexingService abstract class, provides a console application that runs service, and an api.
/// The IndexingService is a Kafka consumer which pulls indexing.
/// </summary>
public class IndexingService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IndexingService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public IndexingService(string[] args) : base(args)
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
            .Configure<IndexingOptions>(this.Configuration.GetSection("Service"))
            .Configure<ProducerConfig>(this.Configuration.GetSection("Kafka:Producer"))
            .Configure<AdminClientConfig>(this.Configuration.GetSection("Kafka:Admin"))
            .AddTransient<IKafkaAdmin, KafkaAdmin>()
            .AddTransient<IKafkaListener<string, IndexRequest>, KafkaListener<string, IndexRequest>>()
            .AddTransient<IKafkaMessenger, KafkaMessenger>()
            .AddSingleton<IServiceManager, IndexingManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<IndexingOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
