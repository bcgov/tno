using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Kafka.Serializers;

namespace TNO.Kafka;

/// <summary>
/// KafkaListener class, provides a way to consume messages from Kafka.
/// </summary>
/// <typeparam name="TKey">The message key type</typeparam>
/// <typeparam name="TValue">The message type</typeparam>
public class KafkaListener<TKey, TValue> : IKafkaListener<TKey, TValue>, IDisposable
{
    #region Variable
    private readonly ConsumerConfig _config;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    private bool _disposed = false;
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

    /// <summary>
    /// get - Whether the listener is actively consuming messages.
    /// </summary>
    public bool IsConsuming { get; private set; }

    /// <summary>
    /// get - An array of topics this consumer is subscribed to.
    /// </summary>
    public string[] Topics { get; private set; } = Array.Empty<string>();
    #endregion

    #region Events
    /// <summary>
    /// Error event will fire when an error occurs.
    /// </summary>
    public event OnConsumerErrorEventHandler? OnError;

    /// <summary>
    /// Error event will fire when an stop occurs.
    /// </summary>
    public event OnConsumerStopEventHandler? OnStop;
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
    /// <param name="topics"></param>
    /// <returns></returns>
    public void Subscribe(params string[] topics)
    {
        if (topics == null) throw new ArgumentNullException(nameof(topics));
        if (topics.Length == 0) throw new ArgumentException("Parameter must have at least one value", nameof(topics));
        if (!this.IsReady) throw new InvalidOperationException("Consumer is not ready for subscribing");

        // Only update the subscription if it changes.
        if (!this.Topics.SequenceEqual(topics))
        {
            this.Topics = topics;

            // TODO: Use the KafkaAdminClient to validate all topics before attempting to subscribe to them.
            this.Consumer.Subscribe(topics);
            _logger.LogInformation("Subscribing to topics: {topics}", String.Join(", ", topics));
        }
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
            if (!this.IsReady) throw new InvalidOperationException("Consumer is not ready for consuming, it must be opened first.");

            this.IsConsuming = true;
            _logger.LogDebug("Waiting to receive message from Kafka topics:'{topic}'", String.Join(", ", this.Consumer.Subscription));

            // I don't understand why Kafka didn't use an async+await pattern here, but this function blocks until it receives a message.
            result = this.Consumer.Consume(cancellationToken);

            _logger.LogInformation("Message received from Kafka topic:'{topic}' key:'{key}'", result.Topic, result.Message.Key);

            // TODO: Pausing is only required if the action takes too long.  This current implementation isn't efficient.
            // It's unclear if this only pauses for a single topic/partition.  If it does then it won't truly pause fully which could lead to issues.
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
            // An unhandled exception will stop consuming as I don't know of a way to resume the paused consumer.
            _logger.LogError(ex, $"Unexpected exception while consuming");
            OnError?.Invoke(this, new ErrorEventArgs(ex));
            this.Stop();
        }
        finally
        {
            // Resume if possible.
            if (result != null && this.IsConsuming)
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
        if (this.IsConsuming && this.IsReady)
        {
            this.IsConsuming = false;
            this.IsReady = false;
            this.Consumer.Close();
            OnStop?.Invoke(this, new EventArgs());
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
