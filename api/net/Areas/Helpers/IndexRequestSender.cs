using Microsoft.Extensions.Options;
using TNO.API.Config;
using TNO.DAL;
using TNO.Kafka;
using TNO.Kafka.Models;

namespace TNO.API.Helpers;

/// <summary>
/// IndexRequestSender class, sends the index requests a context has saved to the Kafka index topic,
/// keyed by content ID. A failure to send is an error: when Kafka is down the request fails.
/// </summary>
public class IndexRequestSender : IIndexRequestSender
{
    #region Variables
    private readonly IKafkaMessenger _kafka;
    private readonly KafkaOptions _options;
    private readonly ILogger<IndexRequestSender> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IndexRequestSender object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafka"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public IndexRequestSender(IKafkaMessenger kafka, IOptions<KafkaOptions> options, ILogger<IndexRequestSender> logger)
    {
        _kafka = kafka;
        _options = options.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Send the index requests the context has saved since the last send.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task SendAsync(TNOContext context)
    {
        var requests = context.TakeIndexRequests();
        if (requests.Count == 0) return;
        if (String.IsNullOrWhiteSpace(_options.IndexingTopic))
        {
            _logger.LogWarning("Kafka indexing topic not configured.");
            return;
        }

        foreach (var request in requests)
        {
            await _kafka.SendMessageAsync(_options.IndexingTopic, new IndexRequestModel(request.ContentId, request.RequestorId, (IndexAction)request.Action)
            {
                ProjectionRevision = request.ProjectionRevision,
                Reason = request.Reason,
            });
        }
    }
    #endregion
}
