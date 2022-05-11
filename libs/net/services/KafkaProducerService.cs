using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Models.Kafka;
using TNO.Services.Serializers;

namespace TNO.Services;

/// <summary>
/// KafkaProducerService class, provides a way to publish messages to Kafka.
/// </summary>
public class KafkaProducerService : IKafkaProducerService
{
    #region Variable
    private readonly ProducerConfig _config;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates new instance of a KafkaProducerService object, initializes with specified parameters.
    /// </summary>
    /// <param name="serializerOptions"></param>
    /// <param name="producerConfigOptions"></param>
    /// <param name="logger"></param>
    public KafkaProducerService(IOptions<JsonSerializerOptions> serializerOptions, IOptions<ProducerConfig> producerConfigOptions, ILogger<KafkaProducerService> logger)
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
        if (String.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Parameter cannot be null, empty, or whitespace", nameof(topic));
        if (content == null) throw new ArgumentNullException(nameof(content));

        return await SendMessageAsync(topic, $"{content.Source}-{content.Uid}", content);
    }
    #endregion
}
