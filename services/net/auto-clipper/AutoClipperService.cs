using Microsoft.Extensions.DependencyInjection;
using TNO.Core.Storage;
using TNO.Core.Storage.Configuration;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.AutoClipper.Audio;
using TNO.Services.AutoClipper.Azure;
using TNO.Services.AutoClipper.Config;
using TNO.Services.AutoClipper.LLM;
using TNO.Services.AutoClipper.Pipeline;
using TNO.Services.Runners;


namespace TNO.Services.AutoClipper;

/// <summary>
/// AutoClipperService abstract class, provides a console application that runs service, and an api.
/// The AutoClipperService is a Kafka consumer which pulls content.
/// </summary>
public class AutoClipperService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AutoClipperService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public AutoClipperService(string[] args) : base(args)
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
            .Configure<AutoClipperOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IKafkaListener<string, ClipRequestModel>, KafkaListener<string, ClipRequestModel>>()
            .AddSingleton<IServiceManager, AutoClipperManager>()
            .Configure<S3Options>(this.Configuration.GetSection("S3"))
            .AddSingleton<IS3StorageService, S3StorageService>()
            .AddSingleton<IStationConfigurationService, StationConfigurationService>()
            .AddSingleton<IAudioNormalizer, AudioNormalizer>()
            .AddSingleton<ClipProcessingPipeline>();

        services.AddSingleton<IAzureSpeechTranscriptionService, AzureSpeechTranscriptionService>();
        services.AddHttpClient<IClipSegmentationService, ClipSegmentationService>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<AutoClipperOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
