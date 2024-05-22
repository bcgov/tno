using Microsoft.Extensions.DependencyInjection;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.FileCopy.Config;
using TNO.Services.Runners;

namespace TNO.Services.FileCopy;

/// <summary>
/// FileCopyService abstract class, provides a console application that runs service, and an api.
/// The FileCopyService is a Kafka consumer which pulls content.
/// </summary>
public class FileCopyService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FileCopyService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public FileCopyService(string[] args) : base(args)
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
            .Configure<FileCopyOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IKafkaListener<string, FileRequestModel>, KafkaListener<string, FileRequestModel>>()
            .AddSingleton<IServiceManager, FileCopyManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<FileCopyOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
