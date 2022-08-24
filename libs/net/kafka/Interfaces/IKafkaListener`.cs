using Confluent.Kafka;

namespace TNO.Kafka;

/// <summary>
/// When a consumer error occurs this event will fire.
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
/// <returns>Whether the error should stop the consumer.</returns>
public delegate bool OnConsumerErrorEventHandler(object sender, ErrorEventArgs e);

/// <summary>
/// IKafkaListener interface, provides a way to consume messages to Kafka.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public interface IKafkaListener<TKey, TValue>
{
    /// <summary>
    /// get - Whether the listener is ready to consumer messages.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Error event will fire when an error occurs.
    /// </summary>
    event OnConsumerErrorEventHandler? OnError;

    /// <summary>
    /// Open a connection to Kafka before subscribing to topics.
    /// </summary>
    void Open();

    /// <summary>
    /// Set the topics this listener will subscribe to.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    void Subscribe(params string[] topic);

    /// <summary>
    /// Unsubscribe to current topics.
    /// </summary>
    void Unsubscribe();

    /// <summary>
    /// Listen for a message from Kafka.
    /// Place this in a loop if you want to receive more than one message.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    Task ConsumeAsync(Func<ConsumeResult<TKey, TValue>, Task> action);

    /// <summary>
    /// Listen for a message from Kafka.
    /// Place this in a loop if you want to receive more than one message.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ConsumeAsync(Func<ConsumeResult<TKey, TValue>, Task> action, CancellationToken cancellationToken);

    /// <summary>
    /// Stop consuming messages from Kafka.
    /// </summary>
    void Stop();
}
