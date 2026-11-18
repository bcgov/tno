using Microsoft.Extensions.Options;
using TNO.API.Config;
using TNO.DAL;
using TNO.Kafka;
using TNO.Kafka.Models;

namespace TNO.API.Helpers;

/// <summary>
/// AnalysisRequestSender class, sends analysis requests to the Kafka analysis topics, keyed by
/// content ID so every request for the same content is handled in order. A failure to send is an
/// error: when Kafka is down the request fails.
/// </summary>
public class AnalysisRequestSender : IAnalysisRequestSender
{
    #region Variables
    private readonly IKafkaMessenger _kafka;
    private readonly KafkaOptions _options;
    private readonly ILogger<AnalysisRequestSender> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisRequestSender object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafka"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public AnalysisRequestSender(IKafkaMessenger kafka, IOptions<KafkaOptions> options, ILogger<AnalysisRequestSender> logger)
    {
        _kafka = kafka;
        _options = options.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Send the analysis requests the context has made since the last send to the analysis topic.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task SendAsync(TNOContext context)
    {
        var requests = context.TakeAnalysisRequests();
        if (requests.Count == 0) return;
        if (String.IsNullOrWhiteSpace(_options.AnalysisTopic))
        {
            _logger.LogWarning("Kafka analysis topic not configured.");
            return;
        }
        await SendAsync(_options.AnalysisTopic, requests.Select(r => new AnalysisRequestModel(r.RequestId, r.ContentId, r.InputHash, r.Reason, r.Force, r.RequestedOn)));
    }

    /// <summary>
    /// Send the requests to the specified analysis topic.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="requests"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">The topic is not an analysis topic.</exception>
    public async Task SendAsync(string topic, IEnumerable<AnalysisRequestModel> requests)
    {
        var topics = new[] { _options.AnalysisTopic, _options.AnalysisBackfillTopic, _options.AnalysisRetryTopic, _options.AnalysisDeadLetterTopic };
        if (String.IsNullOrWhiteSpace(topic) || !topics.Contains(topic)) throw new ArgumentException($"'{topic}' is not a configured analysis topic.", nameof(topic));
        var messages = requests.Select(r => new KeyValuePair<string, AnalysisRequestModel>($"{r.ContentId}", r)).ToArray();
        if (messages.Length == 0) return;
        await _kafka.SendMessagesAsync(topic, messages);
    }
    #endregion
}
