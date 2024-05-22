using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using TNO.Kafka;
using TNO.Services.Content.Config;
using TNO.Services.Runners;

namespace TNO.Services.Content;

/// <summary>
/// ContentService abstract class, provides a console application that runs service, and an api.
/// The ContentService is a Kafka consumer which pulls content.
/// </summary>
public class ContentService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public ContentService(string[] args) : base(args)
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
            .Configure<ContentOptions>(this.Configuration.GetSection("Service"))
            .Configure<AdminClientConfig>(this.Configuration.GetSection("Kafka:Admin"))
            .AddSingleton<IKafkaAdmin, KafkaAdmin>()
            .AddSingleton<IServiceManager, ContentManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<ContentOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
