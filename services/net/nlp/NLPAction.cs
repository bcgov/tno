using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Http;
using TNO.Services.Actions;
using TNO.Services.NLP.Config;

namespace TNO.Services.NLP;

/// <summary>
/// NLPAction class, performs the NLP ingestion action.
/// Fetch NLP feed.
/// Send message to Kafka.
/// Inform api of new content.
/// </summary>
public class NLPAction : ServiceAction<NLPOptions>
{
    #region Variables
    private readonly IHttpRequestClient _httpClient;
    private readonly IKafkaMessenger _kafka;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NLPAction, initializes with specified parameters.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="api"></param>
    /// <param name="kafka"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public NLPAction(IHttpRequestClient httpClient, IApiService api, IKafkaMessenger kafka, IOptions<NLPOptions> options, ILogger<NLPAction> logger) : base(api, options)
    {
        _httpClient = httpClient;
        _kafka = kafka;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public override Task PerformActionAsync(IServiceActionManager manager, string? name = null)
    {
        _logger.LogDebug("Performing NLP service action for ...");

        return Task.FromResult(0);
    }
    #endregion
}
