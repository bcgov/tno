namespace TNO.Kafka;

/// <summary>
/// ConsumerAction enum, provides a way to control what the consumer should do after consuming a message.
/// </summary>
public enum ConsumerAction
{
    /// <summary>
    /// Ignore the error and continue to the next message.
    /// </summary>
    Proceed = 0,
    /// <summary>
    /// Retry consuming the message that threw the error.
    /// </summary>
    Retry = 1,
    /// <summary>
    /// Stop the consumer.
    /// </summary>
    Stop = 2
}
