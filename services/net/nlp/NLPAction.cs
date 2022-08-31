using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Http;
using TNO.Kafka;
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
    private readonly ILogger _logger;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka messenger.
    /// </summary>
    protected IKafkaMessenger Producer { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NLPAction, initializes with specified parameters.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="api"></param>
    /// <param name="producer"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public NLPAction(IHttpRequestClient httpClient, IApiService api, IKafkaMessenger producer, IOptions<NLPOptions> options, ILogger<NLPAction> logger) : base(api, options)
    {
        _httpClient = httpClient;
        this.Producer = producer;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task PerformActionAsync<T>(IServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogDebug("Performing NLP service action for ...");

        return Task.FromResult(0);
    }
    #endregion
}
