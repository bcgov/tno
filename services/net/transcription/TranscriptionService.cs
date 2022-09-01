using Microsoft.Extensions.DependencyInjection;
using TNO.Services.Transcription.Config;
using TNO.Services.Runners;
using TNO.Models.Kafka;
using TNO.Kafka;

namespace TNO.Services.Transcription;

/// <summary>
/// TranscriptionService abstrct class, provides a console application that runs service, and an api.
/// The TranscriptionService is a Kafka consumer which pulls content.
/// </summary>
public class TranscriptionService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TranscriptionService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public TranscriptionService(string[] args) : base(args)
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
            .Configure<TranscriptionOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IKafkaListener<string, TranscriptRequest>, KafkaListener<string, TranscriptRequest>>()
            .AddSingleton<IServiceManager, TranscriptionManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<TranscriptionOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
