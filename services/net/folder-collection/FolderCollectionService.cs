using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using TNO.Elastic;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.FolderCollection.Config;
using TNO.Services.Runners;

namespace TNO.Services.FolderCollection;

/// <summary>
/// FolderCollectionService abstract class, provides a console application that runs service, and an api.
/// The FolderCollectionService is a Kafka consumer which pulls indexing.
/// </summary>
public class FolderCollectionService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FolderCollectionService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public FolderCollectionService(string[] args) : base(args)
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
            .Configure<FolderCollectionOptions>(this.Configuration.GetSection("Service"))
            .Configure<AdminClientConfig>(this.Configuration.GetSection("Kafka:Admin"))
            .AddElasticSingleton(this.Configuration, this.Environment)
            .AddSingleton<IKafkaAdmin, KafkaAdmin>()
            .AddTransient<IKafkaListener<string, IndexRequestModel>, KafkaListener<string, IndexRequestModel>>()
            .AddSingleton<IServiceManager, FolderCollectionManager>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<FolderCollectionOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
