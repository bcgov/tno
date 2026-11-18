using Confluent.Kafka;

namespace TNO.Kafka;

/// <summary>
/// IKafkaProducerProvider interface, provides the process-wide Kafka producers.
/// A Kafka producer is thread-safe and expensive to create (it connects to the brokers and, with idempotence,
/// obtains a producer id before its first send), so one is shared for the life of the process.
/// </summary>
public interface IKafkaProducerProvider
{
    /// <summary>
    /// Get the producer for the key and value types. Every type shares one underlying Kafka client.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    IProducer<TKey, TValue> GetProducer<TKey, TValue>();

    /// <summary>
    /// Discard the shared client after a fatal error, so the next send creates a new one.
    /// </summary>
    void Reset();
}
