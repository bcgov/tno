using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.NLP.Config;
using TNO.Services.Runners;

namespace TNO.Services.NLP;

/// <summary>
/// NLPService abstract class, provides a console application that runs service, and an api.
/// </summary>
public class NLPService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NLPService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public NLPService(string[] args) : base(args)
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
            .Configure<NLPOptions>(this.Configuration.GetSection("Service"))
            .Configure<ProducerConfig>(this.Configuration.GetSection("Kafka:Producer"))
            .AddTransient<IKafkaListener<string, NlpRequestModel>, KafkaListener<string, NlpRequestModel>>()
            .AddTransient<IKafkaMessenger, KafkaMessenger>()
            .AddSingleton<IServiceManager, NlpManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<NLPOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
