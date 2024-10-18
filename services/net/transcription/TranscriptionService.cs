using Microsoft.Extensions.DependencyInjection;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Runners;
using TNO.Services.Transcription.Config;
using TNO.DAL.Services;
using TNO.DAL;
using TNO.DAL.Config;
using Microsoft.Extensions.Options;


namespace TNO.Services.Transcription;

/// <summary>
/// TranscriptionService abstract class, provides a console application that runs service, and an api.
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
        // services.AddSingleton(sp => sp.GetRequiredService<IOptions<StorageOptions>>().Value);
        services
            .Configure<TranscriptionOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IKafkaListener<string, TranscriptRequestModel>, KafkaListener<string, TranscriptRequestModel>>()
            .AddSingleton<IServiceManager, TranscriptionManager>()
            .AddSingleton<IS3StorageService, S3StorageService>()
            .AddS3Config(this.Configuration.GetSection("S3"))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<StorageOptions>>().Value);

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<TranscriptionOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
