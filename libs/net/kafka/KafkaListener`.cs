using System.Text.Json;
using System.Collections.Concurrent;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Kafka.Models;
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
    private bool _open = false;
    private ConsumeResult<TKey, TValue>? _currentResult = null;
    private int _currentThread = 0;
    private readonly int _maxThreads = 10;
    private readonly ConcurrentDictionary<ConsumeResult<TKey, TValue>, ConsumeResult<TKey, TValue>> _currentResults = new();
    #endregion

    #region Properties
    /// <summary>
    /// get - The Kafka consumer.
    /// </summary>
    protected IConsumer<TKey, TValue>? Consumer { get; private set; }

    /// <summary>
    /// get - Whether the listener is actively consuming messages.
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

    private bool IsTranscription => _currentResult is ConsumeResult<string, TranscriptRequest>;
    private bool OverLimit => _currentThread > _maxThreads;
    private bool ReachOrOverLimit => _currentThread >= _maxThreads;
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
    public async Task ConsumeAsync(Func<ConsumeResult<TKey, TValue>, Task<ConsumerAction>> action, CancellationToken cancellationToken)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        // Do not continue if the token has been cancelled.
        if (cancellationToken.IsCancellationRequested) return;
        var proceed = ConsumerAction.Stop;
        this.IsConsuming = true;

        try
        {
            if (!_open) this.Open();

            // If not currently working on a result, wait for a new one.
            // This provides a way to keep trying to handle the current result because of some kind of intermittent failure.
            if (_currentResult == null || (IsTranscription && !ReachOrOverLimit))
            {
                _logger.LogDebug("Waiting to receive message from Kafka topics: '{topic}'", String.Join(", ", this.Consumer?.Subscription ?? new List<string>()));

                // I don't understand why Kafka didn't use an async+await pattern here, but this function blocks until it receives a message.
                _currentResult = this.Consumer!.Consume(cancellationToken);

                _logger.LogInformation("Message received from Kafka topic: '{topic}' key:'{key}'", _currentResult.Topic, _currentResult.Message.Key);

                if (IsTranscription) Interlocked.Increment(ref _currentThread);

                if (!this.IsPaused && (!IsTranscription || ReachOrOverLimit))
                {
                    _logger.LogDebug("Pausing consumption: {topics}", String.Join(", ", this.Consumer.Subscription ?? new List<string>()));
                    // TODO: Pausing is only required if the action takes too long.  This current implementation isn't efficient.
                    this.Consumer.Pause(this.Consumer.Assignment);
                    this.IsPaused = true;
                }
            }
            else
            {
                _logger.LogDebug("Retrying prior message from Kafka topic: '{topic}' key:'{key}'", _currentResult.Topic, _currentResult.Message.Key);
            }

            if (_currentResult != null)
            {
                if (!IsTranscription)
                {
                    proceed = await RunActionAsync(action, _currentResult);
                }
                else if (!OverLimit && !_currentResults.ContainsKey(_currentResult))
                {
                    var current = _currentResult;
                    _currentResults[current] = current;
                    _ = Task.Run(async () =>
                    {
                        var proceed = ConsumerAction.Stop;
                        try
                        {
                            proceed = await RunActionAsync(action, _currentResults[current]);
                        }
                        catch (Exception ex)
                        {
                            proceed = HandleException(ex);
                        }
                        finally
                        {
                            FinalCleanUp(proceed);
                        }
                    }, cancellationToken);
                }
            }
            else
            {
                _logger.LogError("Unexpected consumer with no message");
            }
        }
        catch (Exception ex)
        {
            proceed = HandleException(ex);
        }
        finally
        {
            if (!IsTranscription) FinalCleanUp(proceed);
        }
    }

    private async Task<ConsumerAction> RunActionAsync(
        Func<ConsumeResult<TKey, TValue>, Task<ConsumerAction>> action,
        ConsumeResult<TKey, TValue>? _currentResult)
    {
        var result = await action(_currentResult!);
        if (result == ConsumerAction.Proceed)
        {
            // Manually commit to inform Kafka this message has successfully been handled.
            // TODO: Convert to more efficient process and use EnableAutoOffsetStore=false - https://docs.confluent.io/kafka-clients/dotnet/current/overview.html#store-offsets
            if (_config.EnableAutoCommit == false)
            {
                this.Consumer?.Commit(_currentResult);
                _logger.LogDebug("Message committed from Kafka topic:'{topic}' key:'{key}'", _currentResult!.Topic, _currentResult.Message.Key);
            }
            
            if (IsTranscription)
            {
                Interlocked.Decrement(ref _currentThread); 
                _currentResults.TryRemove(_currentResult!, out _);
            }

            _currentResult = null;
        }
        return result;
    }

    private ConsumerAction HandleException(Exception ex)
    {
        // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
        // An unhandled exception will stop consuming as I don't know of a way to resume the paused consumer.
        _logger.LogError(ex, "Error while consuming. {message}", ex.Message);
        return this.OnError?.Invoke(this, new ErrorEventArgs(ex)) ?? ConsumerAction.Stop;
    }

    private void FinalCleanUp(ConsumerAction proceed)
    {
        if (proceed == ConsumerAction.Stop) this.Stop();
        else if (this.IsPaused && proceed == ConsumerAction.Proceed)
        {
            _logger.LogDebug("Resuming consumption: {topics}", String.Join(", ", this.Consumer!.Subscription ?? new List<string>()));
            this.IsPaused = false;
            this.Consumer?.Resume(this.Consumer.Assignment);
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
    public async Task ConsumeAsync(Func<ConsumeResult<TKey, TValue>, Task<ConsumerAction>> action)
    {
        var cancelToken = new CancellationTokenSource();
        await ConsumeAsync(action, cancelToken.Token);
    }

    /// <summary>
    /// Make a request to stop consuming messages.
    /// </summary>
    public void Stop()
    {
        if (_open)
        {
            _logger.LogInformation("Consumer is stopping");
            this.IsConsuming = false;
            this.IsPaused = false;
            this.Consumer?.Close();
            _open = false;
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
