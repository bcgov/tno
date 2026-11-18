using Confluent.Kafka;

namespace TNO.Kafka;

/// <summary>
/// KafkaHubConfig class, provides a way to configure the Kafka Hub back-plane.
/// </summary>
public class KafkaHubConfig
{
    #region Properties
    /// <summary>
    /// get/set - Kafka admin client configuration.
    /// </summary>
    public AdminClientConfig AdminClient { get; set; } = new AdminClientConfig();

    /// <summary>
    /// get/set - Kafka consumer configuration.
    /// </summary>
    public ConsumerConfig Consumer { get; set; } = new ConsumerConfig();

    /// <summary>
    /// get/set - Kafka producer configuration.
    /// </summary>
    public ProducerConfig Producer { get; set; } = new ProducerConfig();

    /// <summary>
    /// get/set - Kafka hub topic to subscribe and push to.
    /// </summary>
    public string HubTopic { get; set; } = "";

    /// <summary>
    /// get/set - Number of milliseconds to delay after an exception occurs while consuming messages.
    /// </summary>
    public int ConsumerExceptionDelayMs { get; set; } = 1000;
    #endregion
}
