using Microsoft.Extensions.DependencyInjection;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.EventHandler.Config;
using TNO.Services.Runners;

namespace TNO.Services.EventHandler;

/// <summary>
/// EventHandlerService abstract class, provides a console application that runs service, and an api.
/// The EventHandlerService is a Kafka consumer which pulls content.
/// </summary>
public class EventHandlerService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a EventHandlerService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public EventHandlerService(string[] args) : base(args)
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
            .Configure<EventHandlerOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IKafkaListener<string, EventScheduleRequestModel>, KafkaListener<string, EventScheduleRequestModel>>()
            .AddSingleton<IServiceManager, EventHandlerManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<EventHandlerOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
