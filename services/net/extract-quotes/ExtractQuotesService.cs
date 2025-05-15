using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.Core.Http;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.ExtractQuotes.Config;
using TNO.Services.ExtractQuotes.LLM;
using TNO.Services.ExtractQuotes.LLM.Clients;
using TNO.Services.ExtractQuotes.LLM.Parsers;
using TNO.Services.ExtractQuotes.LLM.Prompts;
using TNO.Services.ExtractQuotes.LLM.RateLimiting;
using TNO.Services.NLP.ExtractQuotes;
using TNO.Services.Runners;

namespace TNO.Services.ExtractQuotes;

/// <summary>
/// ExtractQuotesService abstract class, provides a console application that runs service, and an api.
/// The ExtractQuotesService is a Kafka consumer which pulls content.
/// </summary>
public class ExtractQuotesService : KafkaConsumerService
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ExtractQuotesService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public ExtractQuotesService(string[] args) : base(args)
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
            .Configure<ExtractQuotesOptions>(this.Configuration.GetSection("Service"))
            .AddTransient<IKafkaListener<string, IndexRequestModel>, KafkaListener<string, IndexRequestModel>>()
            .AddSingleton<IServiceManager, ExtractQuotesManager>()
            .AddSingleton<IHttpRequestClient, HttpRequestClient>()
            .AddMemoryCache();

        // Register the appropriate service based on configuration
        var serviceSection = this.Configuration.GetSection("Service");
        var useLLM = serviceSection.GetValue<bool>("UseLLM");

        // Get a logger to use during configuration
        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<ExtractQuotesService>();

        logger.LogInformation("Service configuration - Using LLM: {useLLM}", useLLM);

        if (useLLM)
        {
            logger.LogInformation("Registering LLM service as quote extraction service");
            services.AddSingleton<ILLMClient, LLMClient>();
            services.AddSingleton<ILLMResponseParser, LLMResponseParser>();
            services.AddSingleton<IPromptGenerator, PromptGenerator>();
            services.AddSingleton<ILLMRateLimiter, LLMRateLimiter>();
            services.AddSingleton<ICoreNLPService, LLMService>();
        }
        else
        {
            logger.LogInformation("Registering CoreNLP service as quote extraction service");
            services.AddSingleton<ICoreNLPService, CoreNLPService>();
        }

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<ExtractQuotesOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        return services;
    }
    #endregion
}
