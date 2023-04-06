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
    #endregion
}
