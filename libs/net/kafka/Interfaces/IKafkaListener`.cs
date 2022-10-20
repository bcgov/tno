using Confluent.Kafka;

namespace TNO.Kafka;

/// <summary>
/// When a consumer error occurs this event will fire.
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
/// <returns>Whether the the process should retry consuming the message.</returns>
public delegate void OnConsumerErrorEventHandler(object sender, ErrorEventArgs e);

/// <summary>
/// When a consumer stop occurs this event will fire.
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void OnConsumerStopEventHandler(object sender, EventArgs e);

/// <summary>
/// IKafkaListener interface, provides a way to consume messages to Kafka.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public interface IKafkaListener<TKey, TValue>
{
    #region Properties
    /// <summary>
    /// get - Whether the listener should await the message handler.
    /// </summary>
    public bool IsLongRunningJob { get; set; }

    /// <summary>
    /// get - Whether the listener is actively consuming messages.
    /// </summary>
    public bool IsConsuming { get; }

    /// <summary>
    /// get - Whether the listener is currently paused but still consuming messages.
    /// </summary>
    public bool IsPaused { get; }

    /// <summary>
    /// get - An array of topics this consumer is subscribed to.
    /// </summary>
    public string[] Topics { get; }

    /// <summary>
    /// get/set - The max threads for running service.
    /// </summary>
    public int MaxThreads { get; set; }
    #endregion

    #region Events
    /// <summary>
    /// Error event will fire when an error occurs.
    /// </summary>
    event OnConsumerErrorEventHandler? OnError;

    /// <summary>
    /// Error event will fire when an stop occurs.
    /// </summary>
    event OnConsumerStopEventHandler? OnStop;
    #endregion

    #region Methods
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
    /// <returns>Whether the consumer should continue with the next message.</returns>
    Task ConsumeAsync(Func<ConsumeResult<TKey, TValue>, Task> action);

    /// <summary>
    /// Listen for a message from Kafka.
    /// Place this in a loop if you want to receive more than one message.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Whether the consumer should continue with the next message.</returns>
    Task ConsumeAsync(Func<ConsumeResult<TKey, TValue>, Task> action, CancellationToken cancellationToken);

    /// <summary>
    /// Get the current position for the specified topic partition.
    /// </summary>
    /// <param name="partition"></param>
    /// <returns></returns>
    Offset? Position(TopicPartition partition);

    /// <summary>
    /// Inform Kafka that the specified 'result' has been completed for this consumer group.
    /// </summary>
    /// <param name="result"></param>
    void Commit(ConsumeResult<TKey, TValue> result);

    /// <summary>
    /// Pause consuming messages from the current assignment.
    /// </summary>
    public void Pause();

    /// <summary>
    /// Pause consuming messages from the specified partitions.
    /// </summary>
    /// <param name="partitions"></param>
    public void Pause(IEnumerable<TopicPartition> partitions);

    /// <summary>
    /// Resume consuming messages from the current assignment.
    /// </summary>
    void Resume();

    /// <summary>
    /// Resume consuming messages from the specified partitions.
    /// </summary>
    /// <param name="partitions"></param>
    void Resume(IEnumerable<TopicPartition> partitions);

    /// <summary>
    /// Stop consuming messages from Kafka.
    /// </summary>
    void Stop();
    #endregion
}
