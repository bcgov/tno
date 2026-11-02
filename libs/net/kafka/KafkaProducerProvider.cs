using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Kafka.Serializers;

namespace TNO.Kafka;

/// <summary>
/// KafkaProducerProvider class, owns the process-wide Kafka producer.
/// One client (an IProducer of raw bytes) holds the broker connections. Each key/value type gets a dependent
/// producer that wraps that client with its JSON serializers, so typed results and exceptions are unchanged.
/// Register it as a singleton. It is disposed when the host stops, which flushes anything still in flight.
/// </summary>
public sealed class KafkaProducerProvider : IKafkaProducerProvider, IDisposable
{
    #region Variables
    /// <summary>
    /// How long to wait for in-flight messages when the process stops.
    /// </summary>
    private static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(10);

    private readonly ProducerConfig _config;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    private readonly object _lock = new();
    private Client? _client;
    private bool _disposed;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KafkaProducerProvider object, initializes with specified parameters.
    /// </summary>
    /// <param name="serializerOptions"></param>
    /// <param name="producerConfigOptions"></param>
    /// <param name="logger"></param>
    public KafkaProducerProvider(IOptions<JsonSerializerOptions> serializerOptions, IOptions<ProducerConfig> producerConfigOptions, ILogger<KafkaProducerProvider> logger)
    {
        _serializerOptions = serializerOptions.Value;
        _config = producerConfigOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the producer for the key and value types. Every type shares one underlying Kafka client.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public IProducer<TKey, TValue> GetProducer<TKey, TValue>()
    {
        var client = GetClient();
        return (IProducer<TKey, TValue>)client.Producers.GetOrAdd((typeof(TKey), typeof(TValue)), _ =>
            new DependentProducerBuilder<TKey, TValue>(client.Handle.Handle)
                .SetKeySerializer(new DefaultJsonSerializer<TKey>(_serializerOptions))
                .SetValueSerializer(new DefaultJsonSerializer<TValue>(_serializerOptions))
                .Build());
    }

    /// <summary>
    /// Discard the shared client after a fatal error, so the next send creates a new one.
    /// </summary>
    public void Reset()
    {
        Client? previous;
        lock (_lock)
        {
            previous = _client;
            _client = null;
        }
        if (previous != null)
        {
            _logger.LogWarning("Kafka producer was reset after a fatal error; the next message creates a new producer.");
            previous.Dispose(TimeSpan.Zero);
        }
    }

    /// <summary>
    /// Flush in-flight messages and release the client.
    /// </summary>
    public void Dispose()
    {
        Client? client;
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;
            client = _client;
            _client = null;
        }
        client?.Dispose(FlushTimeout);
    }

    private Client GetClient()
    {
        var client = Volatile.Read(ref _client);
        if (client != null) return client;
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _client ??= new Client(new ProducerBuilder<byte[], byte[]>(_config)
                .SetErrorHandler((_, error) =>
                {
                    if (error.IsFatal) _logger.LogError("Kafka producer fatal error {code}: {reason}", error.Code, error.Reason);
                    else _logger.LogWarning("Kafka producer error {code}: {reason}", error.Code, error.Reason);
                })
                .Build());
        }
    }
    #endregion

    #region Client
    /// <summary>
    /// The shared client and the typed producers that depend on it, discarded together on a reset.
    /// </summary>
    /// <param name="handle"></param>
    private sealed class Client(IProducer<byte[], byte[]> handle)
    {
        public IProducer<byte[], byte[]> Handle { get; } = handle;

        public ConcurrentDictionary<(Type Key, Type Value), object> Producers { get; } = new();

        public void Dispose(TimeSpan flushTimeout)
        {
            if (flushTimeout > TimeSpan.Zero) Handle.Flush(flushTimeout);
            // Dependent producers do not own the client; dispose them before the client they use.
            foreach (var producer in Producers.Values.OfType<IDisposable>()) producer.Dispose();
            Handle.Dispose();
        }
    }
    #endregion
}
