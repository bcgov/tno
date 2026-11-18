using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.ContentAnalysis.Config;
using TNO.Services.Runners;

namespace TNO.Services.ContentAnalysis;

/// <summary>
/// ContentAnalysisService class, provides a console host for the Content-Analysis workers.
/// </summary>
public class ContentAnalysisService : KafkaConsumerService
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisService object.
    /// </summary>
    /// <param name="args"></param>
    public ContentAnalysisService(string[] args) : base(args)
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
            .Configure<ContentAnalysisOptions>(this.Configuration.GetSection("Service"))
            .Configure<AdminClientConfig>(this.Configuration.GetSection("Kafka:Admin"))
            .AddSingleton<IKafkaAdmin, KafkaAdmin>()
            .AddTransient<IKafkaListener<string, AnalysisRequestModel>, KafkaListener<string, AnalysisRequestModel>>()
            .AddSingleton<IServiceManager, ContentAnalysisManager>();

        return services;
    }
    #endregion
}
