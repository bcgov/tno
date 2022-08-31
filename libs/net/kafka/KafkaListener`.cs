using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Kafka.Serializers;

namespace TNO.Kafka;

/// <summary>
/// KafkaListener class, provides a way to consume messages from Kafka.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class KafkaListener<TKey, TValue> : IKafkaListener<TKey, TValue>, IDisposable
{
    #region Variable
    private readonly ConsumerConfig _config;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    private bool _disposed = false;
    private bool _consuming = false;
    #endregion

    #region Properties
    /// <summary>
    /// get - The Kafka consumer.
    /// </summary>
    protected IConsumer<TKey, TValue> Consumer { get; private set; }

    /// <summary>
    /// get - Whether the listener is ready to consumer messages.
    /// </summary>
    public bool IsReady { get; private set; }
    #endregion

    #region Events
    /// <summary>
    /// Error event will fire when an error occurs.
    /// </summary>
    public event OnConsumerErrorEventHandler? OnError;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates new instance of a KafkaListener object, initializes with specified parameters.
    /// </summary>
    /// <param name="serializerOptions"></param>
    /// <param name="consumerConfigOptions"></param>
    /// <param name="logger"></param>
    public KafkaListener(IOptions<JsonSerializerOptions> serializerOptions, IOptions<ConsumerConfig> consumerConfigOptions, ILogger<KafkaListener<TKey, TValue>> logger)
    {
        _serializerOptions = serializerOptions.Value;
        _config = consumerConfigOptions.Value;
        _logger = logger;

        this.Consumer = Build();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Build a new Kafka consumer based on configuration.
    /// </summary>
    /// <returns></returns>
    private IConsumer<TKey, TValue> Build()
    {
        var builder = new ConsumerBuilder<TKey, TValue>(_config);
        if (typeof(TKey).IsClass && typeof(TKey) != typeof(string))
            builder.SetKeyDeserializer(new DefaultJsonSerializer<TKey>(_serializerOptions));
        if (typeof(TValue).IsClass && typeof(TValue) != typeof(string))
            builder.SetValueDeserializer(new DefaultJsonSerializer<TValue>(_serializerOptions));

        return builder.Build();
    }

    /// <summary>
    /// Open a connection to Kafka before subscribing to topics.
    /// </summary>
    public void Open()
    {
        this.Consumer = Build();
        this.IsReady = true;
    }

    /// <summary>
    /// Listen for a message from Kafka.
    /// Place this in a loop if you want to receive more than one message.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public void Subscribe(params string[] topic)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        if (topic.Length == 0) throw new ArgumentException("Parameter must have at least one value", nameof(topic));
        if (!this.IsReady) throw new InvalidOperationException("Consumer is not ready for subscribing");

        // TODO: Use the KafkaAdminClient to validate all topics before attempting to subscribe to them.
        this.Consumer.Subscribe(topic);
        _logger.LogInformation("Consuming topics: {tps}", String.Join(", ", topic));
    }

    /// <summary>
    /// Unsubscribe to current topics.
    /// </summary>
    public void Unsubscribe()
    {
        this.Consumer.Unsubscribe();
    }

    /// <summary>
    /// Listen for a message from Kafka.
    /// Place this in a loop if you want to receive more than one message.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task ConsumeAsync(Func<ConsumeResult<TKey, TValue>, Task> action, CancellationToken cancellationToken)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        ConsumeResult<TKey, TValue>? result = null;

        try
        {
            // Ensure consumer is ready for consumption.
            if (!this.IsReady) throw new InvalidOperationException("Consumer is not ready for consuming");

            _consuming = true;
            // I don't understand why Kafka didn't use an async+await pattern here, but this function blocks until it receives a message.
            result = this.Consumer.Consume(cancellationToken);

            _logger.LogInformation("Message received from Kafka topic:'{topic}' key:'{key}'", result.Topic, result.Message.Key);

            // TODO: Pausing is only required if the action takes too long.  This current implementation isn't efficient.
            this.Consumer.Pause(new[] { result.TopicPartition });

            await action(result);
        }
        catch (ConsumeException ex)
        {
            // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
            _logger.LogError(ex, "Consumer error: {Message}", ex.Message);
            this.IsReady = (!OnError?.Invoke(this, new ErrorEventArgs(ex)) ?? true) && this.IsReady;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected exception while consuming");
            this.IsReady = (!OnError?.Invoke(this, new ErrorEventArgs(ex)) ?? true) && this.IsReady;
        }
        finally
        {
            // Resume unless the error is consider fatal.
            if (result != null && this.IsReady)
                this.Consumer.Resume(new[] { result.TopicPartition });
        }
    }

    /// <summary>
    /// Listen for messages from Kafka for the specified topics.
    /// This will pause and resume after receiving every message.
    /// This isn't meant to be performant.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task ConsumeAsync(Func<ConsumeResult<TKey, TValue>, Task> action)
    {
        var cancelToken = new CancellationTokenSource();
        await ConsumeAsync(action, cancelToken.Token);
    }

    /// <summary>
    /// Make a request to stop consuming messages.
    /// </summary>
    public void Stop()
    {
        _logger.LogInformation("Consumer is stopping");
        var isReady = this.IsReady;
        this.IsReady = false;
        if (_consuming && isReady)
        {
            _consuming = false;
            this.Consumer.Close();
        }
    }

    /// <summary>
    /// Close the open consumer.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Close the open consumer.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Stop();
                this.Consumer.Dispose();
            }
            _disposed = true;
        }
    }
    #endregion
}
