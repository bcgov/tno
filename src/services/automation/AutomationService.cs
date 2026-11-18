using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using TNO.AI;
using TNO.Elastic;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Automation.Config;
using TNO.Services.Runners;

namespace TNO.Services.Automation;

/// <summary>
/// AutomationService class, provides a console host for automation workflows.
/// Extends KafkaConsumerService so the Kafka consumer configuration is bound.
/// </summary>
public class AutomationService : KafkaConsumerService
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationService object.
    /// </summary>
    /// <param name="args"></param>
    public AutomationService(string[] args) : base(args)
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
            .Configure<AutomationOptions>(this.Configuration.GetSection("Service"))
            .Configure<AzureAIOptions>(this.Configuration.GetSection("AzureAI"))
            .Configure<AdminClientConfig>(this.Configuration.GetSection("Kafka:Admin"))
            .AddSingletonElastic(this.Configuration, this.Environment)
            .AddSingleton<IAIAgentClient, AIAgentClient>()
            .AddSingleton<IKafkaAdmin, KafkaAdmin>()
            .AddTransient<IKafkaListener<string, AutomationRequestModel>, KafkaListener<string, AutomationRequestModel>>()
            .AddSingleton<IServiceManager, AutomationManager>();

        return services;
    }
    #endregion
}
