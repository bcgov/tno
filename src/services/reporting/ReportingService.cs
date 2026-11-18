using Microsoft.Extensions.DependencyInjection;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Reporting.Config;
using TNO.Services.Runners;
using TNO.TemplateEngine;

namespace TNO.Services.Reporting;

/// <summary>
/// ReportingService abstract class, provides a console application that runs service, and an api.
/// The ReportingService is a Kafka consumer which pulls content.
/// </summary>
public class ReportingService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportingService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public ReportingService(string[] args) : base(args)
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
            .Configure<ReportingOptions>(this.Configuration.GetSection("Service"))
            // One in-flight report prevents a later commit skipping an earlier failed message.
            // Bounded polls let the watchdog distinguish an idle queue from a stopped consumer.
            .PostConfigure<KafkaConsumerConfig>(options =>
            {
                options.MaxThreads = 1;
                options.EnableAutoCommit = false;
                options.ConsumeTimeoutMs = 1000;
                options.ReleaseHandlerOnCommitFailure = true;
            })
            .AddTransient<IKafkaListener<string, ReportRequestModel>, KafkaListener<string, ReportRequestModel>>()
            .AddSingleton<IServiceManager, ReportingManager>()
            .AddSingleton<TNO.TemplateEngine.IReportAIResultStore, ApiReportAIResultStore>()
            .AddSingleton<TNO.TemplateEngine.IReportEvidenceProvider, ApiReportEvidenceProvider>()
            .AddTemplateEngineSingleton(this.Configuration);

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<ReportingOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
