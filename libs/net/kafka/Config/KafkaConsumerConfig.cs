using Confluent.Kafka;

namespace TNO.Kafka;

/// <summary>
/// KafkaHubConfig class, provides a way to configure the Kafka Hub back-plane.
/// </summary>
public class KafkaConsumerConfig : ConsumerConfig
{
    #region Properties
    /// <summary>
    /// get/set - The max threads for handling messages.
    /// </summary>
    public int MaxThreads { get; set; } = 10;

    /// <summary>
    /// get/set - Maximum poll wait in milliseconds, or zero to wait until cancellation.
    /// A bounded wait allows a worker to monitor its consumer loop even on an empty topic.
    /// </summary>
    public int ConsumeTimeoutMs { get; set; }

    /// <summary>
    /// get/set - Release the completed handler slot even if committing fails.
    /// Enable only for workers that stop on commit failure; callers that recover with Seek
    /// leave this false because Seek releases their slot.
    /// </summary>
    public bool ReleaseHandlerOnCommitFailure { get; set; }
    #endregion
}
