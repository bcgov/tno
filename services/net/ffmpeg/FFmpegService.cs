using Microsoft.Extensions.DependencyInjection;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.FFmpeg.Config;
using TNO.Services.Runners;

namespace TNO.Services.FFmpeg;

/// <summary>
/// FFmpegService abstract class, provides a console application that runs service, and an api.
/// The FFmpegService is a Kafka consumer which pulls content.
/// </summary>
public class FFmpegService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FFmpegService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public FFmpegService(string[] args) : base(args)
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
            .Configure<FFmpegOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IKafkaListener<string, IndexRequestModel>, KafkaListener<string, IndexRequestModel>>()
            .AddSingleton<IServiceManager, FFmpegManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<FFmpegOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
