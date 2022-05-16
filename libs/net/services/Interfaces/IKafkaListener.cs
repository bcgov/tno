using Confluent.Kafka;
using TNO.Models.Kafka;

namespace TNO.Services;

/// <summary>
/// IKafkaListener interface, provides a way to consume messages to Kafka.
/// </summary>
public interface IKafkaListener
{
    /// <summary>
    /// Listen for messages from Kafka for the specified topics.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="action"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    Task ListenAsync<TKey, TValue>(Func<ConsumeResult<TKey, TValue>, Task> action, params string[] topic);

    /// <summary>
    /// Listen for messages from Kafka for the specified topics.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    Task ListenAsync<TKey, TValue>(Func<ConsumeResult<TKey, TValue>, Task> action, CancellationToken cancellationToken, params string[] topic);

    /// <summary>
    /// Listen for messages from Kafka for the specified topics.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    Task ListenAsync(Func<ConsumeResult<string, SourceContent>, Task> action, params string[] topic);

    /// <summary>
    /// Listen for messages from Kafka for the specified topics.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    Task ListenAsync(Func<ConsumeResult<string, SourceContent>, Task> action, CancellationToken cancellation, params string[] topic);
}
