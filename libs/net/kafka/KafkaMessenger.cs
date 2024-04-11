using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Kafka.Models;
using TNO.Kafka.Serializers;
using TNO.Kafka.SignalR;

namespace TNO.Kafka;

/// <summary>
/// KafkaMessenger class, provides a way to publish messages to Kafka.
/// </summary>
public class KafkaMessenger : IKafkaMessenger
{
    #region Variable
    private readonly ProducerConfig _config;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates new instance of a KafkaMessenger object, initializes with specified parameters.
    /// </summary>
    /// <param name="serializerOptions"></param>
    /// <param name="producerConfigOptions"></param>
    /// <param name="logger"></param>
    public KafkaMessenger(IOptions<JsonSerializerOptions> serializerOptions, IOptions<ProducerConfig> producerConfigOptions, ILogger<KafkaMessenger> logger)
    {
        _serializerOptions = serializerOptions.Value;
        _config = producerConfigOptions.Value;
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

        var builder = new ProducerBuilder<TKey, TValue>(_config);
        builder.SetKeySerializer(new DefaultJsonSerializer<TKey>(_serializerOptions));
        builder.SetValueSerializer(new DefaultJsonSerializer<TValue>(_serializerOptions));
        using var producer = builder.Build();

        _logger.LogDebug("Sending message to Kafka topic:'{topic}' key:'{key}'", topic, key);
        var message = new Message<TKey, TValue>()
        {
            Key = key,
            Value = value
        };
        var result = await producer.ProduceAsync(topic, message);
        _logger.LogDebug("Message received by Kafka topic:'{topic}' key:'{key}'", topic, key);
        return result;
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
    public async Task<DeliveryResult<string, NlpRequestModel>?> SendMessageAsync(string topic, NlpRequestModel request)
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
