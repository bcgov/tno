using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Models.Kafka;
using TNO.Services.Serializers;

namespace TNO.Services;

/// <summary>
/// KafkaListener class, provides a way to publish messages to Kafka.
/// </summary>
public class KafkaListener : IKafkaListener
{
    #region Variable
    private readonly ConsumerConfig _config;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    #endregion

    #region Properties
    /// <summary>
    /// get/set - Whether the listern is currently running.
    /// </summary>
    public bool IsRunning { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates new instance of a KafkaListener object, initializes with specified parameters.
    /// </summary>
    /// <param name="serializerOptions"></param>
    /// <param name="consumerConfigOptions"></param>
    /// <param name="logger"></param>
    public KafkaListener(IOptions<JsonSerializerOptions> serializerOptions, IOptions<ConsumerConfig> consumerConfigOptions, ILogger<KafkaListener> logger)
    {
        _serializerOptions = serializerOptions.Value;
        _config = consumerConfigOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Listen for messages from Kafka for the specified topics.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task ListenAsync<TKey, TValue>(Func<ConsumeResult<TKey, TValue>, Task> action, CancellationToken cancellationToken, params string[] topic)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        if (topic.Length == 0) throw new ArgumentException("Parameter must have more than one value", nameof(topic));

        var builder = new ConsumerBuilder<TKey, TValue>(_config);
        if (typeof(TKey).IsClass && typeof(TKey) != typeof(string))
            builder.SetKeyDeserializer(new DefaultJsonSerializer<TKey>(_serializerOptions));
        if (typeof(TValue).IsClass && typeof(TValue) != typeof(string))
            builder.SetValueDeserializer(new DefaultJsonSerializer<TValue>(_serializerOptions));
        using var consumer = builder.Build();
        consumer.Subscribe(topic);
        this.IsRunning = true;

        try
        {

            while (this.IsRunning)
            {
                var result = consumer.Consume(cancellationToken);

                _logger.LogDebug("Message received from Kafka topic:'{topic}' key:'{key}'", result.Topic, result.Message.Key);

                // TODO: Handle failures.
                await action(result);
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            this.IsRunning = false;
            consumer.Close();
        }
    }

    /// <summary>
    /// Listen for messages from Kafka for the specified topics.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="action"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task ListenAsync<TKey, TValue>(Func<ConsumeResult<TKey, TValue>, Task> action, params string[] topic)
    {
        var cancelToken = new CancellationTokenSource();
        await ListenAsync(action, cancelToken.Token, topic);
    }

    /// <summary>
    /// Listen for messages from Kafka for the specified topics.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task ListenAsync(Func<ConsumeResult<string, SourceContent>, Task> action, params string[] topic)
    {
        await ListenAsync<string, SourceContent>(action, topic);
    }

    /// <summary>
    /// Listen for messages from Kafka for the specified topics.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task ListenAsync(Func<ConsumeResult<string, SourceContent>, Task> action, CancellationToken cancellation, params string[] topic)
    {
        await ListenAsync<string, SourceContent>(action, cancellation, topic);
    }
    #endregion
}
