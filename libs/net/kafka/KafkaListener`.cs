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
    private readonly KafkaConsumerConfig _config;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    private bool _disposed = false;
    private bool _open = false;
    private int _resultConsumerTracker = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - The Kafka consumer.
    /// </summary>
    protected IConsumer<TKey, TValue>? Consumer { get; private set; }

    /// <summary>
    /// get - Whether the listener should await the message handler.
    /// If 'true' you will need to Commit and Resume after handling a message.
    /// </summary>
    public bool IsLongRunningJob { get; set; }

    /// <summary>
    /// get - Whether the listener is currently consuming messages.
    /// </summary>
    public bool IsConsuming { get; private set; }

    /// <summary>
    /// get - Whether the listener is currently paused but still consuming messages.
    /// </summary>
    public bool IsPaused { get; private set; }

    /// <summary>
    /// get - An array of topics this consumer is subscribed to.
    /// </summary>
    public string[] Topics { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// get - Determine if max threads has been reached.
    /// </summary>
    private bool ReachOrOverLimit => _resultConsumerTracker >= _config.MaxThreads;
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
    public KafkaListener(IOptions<JsonSerializerOptions> serializerOptions, IOptions<KafkaConsumerConfig> consumerConfigOptions, ILogger<KafkaListener<TKey, TValue>> logger)
    {
        _serializerOptions = serializerOptions.Value;
        _config = consumerConfigOptions.Value;
        _logger = logger;
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
        _open = true;
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
        if (!_open) this.Open();

        // Only update the subscription if it changes.
        if (!this.Topics.SequenceEqual(topics))
        {
            this.Topics = topics;
            this.Consumer?.Subscribe(topics);
            _logger.LogInformation("Subscribing to topics: {topics}", String.Join(", ", topics));
        }
    }

    /// <summary>
    /// Unsubscribe to current topics.
    /// </summary>
    public void Unsubscribe()
    {
        this.Consumer?.Unsubscribe();
    }

    /// <summary>
    /// Listen for a message from Kafka.
    /// Place this in a loop if you want to receive more than one message.
    /// Wrapped in a try+catch so that consuming failures should never result in a dead thread.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task ConsumeAsync(Func<ConsumeResult<TKey, TValue>, Task> action, CancellationToken cancellationToken)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        // Do not continue if the token has been cancelled.
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        this.IsConsuming = true;

        try
        {
            if (!_open) this.Open();

            if (!this.IsPaused)
                _logger.LogDebug("Waiting to receive message from Kafka topics: '{topic}'", String.Join(", ", this.Consumer?.Subscription ?? new List<string>()));

            // I don't understand why Kafka didn't use an async+await pattern here, but this function blocks until it receives a message.
            var consumeResult = this.Consumer?.Consume(cancellationToken);
            if (consumeResult != null)
            {
                _logger.LogDebug("Message received from Kafka topic: '{topic}' key:'{key}'", consumeResult.Topic, consumeResult.Message.Key);
                if (this.IsLongRunningJob)
                {
                    Interlocked.Increment(ref _resultConsumerTracker);
                    if (!this.IsPaused && ReachOrOverLimit) Pause();

                    // Cannot await for the action to complete because if it takes too long the Kafka consumer will leave the group.
                    // If the consumer receives another message before the prior action completes it'll result in numerous orphaned threads.
                    // Either we need to maintain a dictionary, throw an error, or limit the number.
                    // When `IsLongRunningJob` it should pause consumption.
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await action(consumeResult);
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex);
                        }
                    }, cancellationToken);
                }
                else
                    await action(consumeResult);
            }
            else
            {
                _logger.LogWarning("Unexpected consume containing no message");
            }
        }
        catch (ConsumeException ex)
        {
            // No need to thro
            if (ex.Message.Contains("Subscribed topic not available:"))
                _logger.LogWarning(ex, "Subscribed topic not available: {topic}: Broker: Unknown topic or partition", this.Consumer?.Subscription);
            else
                HandleException(ex);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void HandleException(Exception ex)
    {
        // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
        // An unhandled exception will stop consuming as I don't know of a way to resume the paused consumer.
        _logger.LogError(ex, "Error while consuming. {message}", ex.Message);
        this.OnError?.Invoke(this, new ErrorEventArgs(ex));
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
    /// Inform Kafka that the specified 'result' has been completed for this consumer group.
    /// </summary>
    /// <param name="result"></param>
    public void Commit(ConsumeResult<TKey, TValue> result)
    {
        this.Consumer?.Commit(result);
        _logger.LogDebug("Message committed from topic:'{topic}' key:'{key}'", result.Topic, result.Message.Key);

        if (this.IsLongRunningJob) Interlocked.Decrement(ref _resultConsumerTracker);
    }

    /// <summary>
    /// Pause consuming messages from the current assignment.
    /// </summary>
    public void Pause()
    {
        if (this.Consumer != null)
            Pause(this.Consumer.Assignment);
    }

    /// <summary>
    /// Pause consuming messages from the specified partitions.
    /// </summary>
    /// <param name="partitions"></param>
    public void Pause(IEnumerable<TopicPartition> partitions)
    {
        if (!this.IsPaused)
        {
            _logger.LogDebug("Pausing consumption: {topics}", String.Join(", ", partitions.Select(p => p.Topic).Distinct()));
            this.IsPaused = true;
            this.Consumer?.Pause(partitions);
        }
    }

    /// <summary>
    /// Resume consuming messages from the current assignment.
    /// </summary>
    public void Resume()
    {
        if (this.Consumer != null)
            Resume(this.Consumer.Assignment);
    }

    /// <summary>
    /// Resume consuming messages from the specified partitions.
    /// </summary>
    /// <param name="partitions"></param>
    public void Resume(IEnumerable<TopicPartition> partitions)
    {
        if (this.IsPaused || !this.IsConsuming)
        {
            _logger.LogDebug("Resuming consumption: {topics}", String.Join(", ", partitions.Select(p => p.Topic).Distinct()));
            this.IsPaused = false;
            this.Consumer?.Resume(partitions);
        }
    }

    /// <summary>
    /// Get the position for the specific topic partition.
    /// </summary>
    /// <returns></returns>
    public Offset? Position(TopicPartition partition)
    {
        return this.Consumer?.Position(partition);
    }

    /// <summary>
    /// Make a request to stop consuming messages.
    /// </summary>
    public void Stop()
    {
        if (_open)
        {
            _logger.LogInformation("Consumer is stopping");
            this.Topics = Array.Empty<string>();
            this.IsConsuming = false;
            this.IsPaused = false;
            _open = false;
            // this.Consumer?.Close();
            this.Consumer?.Unassign();
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
                this.Consumer?.Dispose();
            }
            _disposed = true;
        }
    }
    #endregion
}
