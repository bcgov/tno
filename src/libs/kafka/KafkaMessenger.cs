using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;

namespace TNO.Kafka;

/// <summary>
/// KafkaMessenger class, provides a way to publish messages to Kafka.
/// Messages are sent with the process-wide producer from <see cref="IKafkaProducerProvider"/>, so this class is
/// cheap to create and can be registered with any lifetime.
/// </summary>
public class KafkaMessenger : IKafkaMessenger
{
    #region Variable
    private readonly IKafkaProducerProvider _producers;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates new instance of a KafkaMessenger object, initializes with specified parameters.
    /// </summary>
    /// <param name="producers"></param>
    /// <param name="logger"></param>
    public KafkaMessenger(IKafkaProducerProvider producers, ILogger<KafkaMessenger> logger)
    {
        _producers = producers;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="topic"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<TKey, TValue>?> SendMessageAsync<TKey, TValue>(string topic, TKey key, TValue value)
    {
        if (String.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Parameter cannot be null, empty, or whitespace", nameof(topic));
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (value == null) throw new ArgumentNullException(nameof(value));

        _logger.LogDebug("Sending message to Kafka topic:'{topic}' key:'{key}'", topic, key);
        var message = new Message<TKey, TValue>()
        {
            Key = key,
            Value = value
        };
        var result = await ProduceAsync<TKey, TValue, DeliveryResult<TKey, TValue>>(producer => producer.ProduceAsync(topic, message));
        _logger.LogDebug("Message received by Kafka topic:'{topic}' key:'{key}'", topic, key);
        return result;
    }

    /// <summary>
    /// Send the messages to Kafka, waiting until every message is accepted.
    /// Throws when any message is not.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="topic"></param>
    /// <param name="messages"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<TKey, TValue>[]> SendMessagesAsync<TKey, TValue>(string topic, IEnumerable<KeyValuePair<TKey, TValue>> messages)
    {
        if (String.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Parameter cannot be null, empty, or whitespace", nameof(topic));
        ArgumentNullException.ThrowIfNull(messages);

        var results = await ProduceAsync<TKey, TValue, DeliveryResult<TKey, TValue>[]>(producer =>
            Task.WhenAll(messages.Select(m => producer.ProduceAsync(topic, new Message<TKey, TValue>() { Key = m.Key, Value = m.Value }))));
        _logger.LogDebug("Messages received by Kafka topic:'{topic}' count:{count}", topic, results.Length);
        return results;
    }

    /// <summary>
    /// Send with the shared producer. A fatal error leaves a Kafka producer unusable, so it is discarded and
    /// the next send creates a new one; the error is still thrown to the caller.
    /// </summary>
    private async Task<TResult> ProduceAsync<TKey, TValue, TResult>(Func<IProducer<TKey, TValue>, Task<TResult>> send)
    {
        try
        {
            return await send(_producers.GetProducer<TKey, TValue>());
        }
        catch (KafkaException ex) when (ex.Error.IsFatal)
        {
            _producers.Reset();
            throw;
        }
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, SourceContent>?> SendMessageAsync(string topic, SourceContent content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        return await SendMessageAsync(topic, $"{content.Source}-{content.Uid}", content);
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, TranscriptRequestModel>?> SendMessageAsync(string topic, TranscriptRequestModel request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return await SendMessageAsync(topic, $"{request.ContentId}", request);
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, ClipRequestModel>?> SendMessageAsync(string topic, ClipRequestModel request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return await SendMessageAsync(topic, $"{request.ContentId}", request);
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, IndexRequestModel>?> SendMessageAsync(string topic, IndexRequestModel request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return await SendMessageAsync(topic, $"{request.ContentId}", request);
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, FileRequestModel>?> SendMessageAsync(string topic, FileRequestModel request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return await SendMessageAsync(topic, $"{request.LocationId}:{request.Path}", request);
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, KafkaHubMessage>?> SendMessageAsync(string topic, KafkaHubMessage request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return await SendMessageAsync(topic, $"{request.HubEvent}-${DateTime.Now.Ticks}", request);
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, NotificationRequestModel>?> SendMessageAsync(string topic, NotificationRequestModel request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        var key = "";
        if (request.NotificationId.HasValue && request.ContentId.HasValue) key = $"notification-{request.NotificationId}:content={request.ContentId}";
        else if (request.NotificationId.HasValue) key = $"notification-{request.NotificationId}";
        else if (request.ContentId.HasValue) key = $"content-{request.ContentId}";

        return await SendMessageAsync(topic, key, request);
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, ReportRequestModel>?> SendMessageAsync(string topic, ReportRequestModel request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return await SendMessageAsync(topic, $"report-{request.ReportId}", request);
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, EventScheduleRequestModel>?> SendMessageAsync(string topic, EventScheduleRequestModel request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return await SendMessageAsync(topic, $"event-{request.EventScheduleId}", request);
    }

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<DeliveryResult<string, FFmpegRequestModel>?> SendMessageAsync(string topic, FFmpegRequestModel request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return await SendMessageAsync(topic, $"ffmpeg-{request.ContentId}", request);
    }
    #endregion
}
