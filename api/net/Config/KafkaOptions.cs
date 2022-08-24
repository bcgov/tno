namespace TNO.API.Config;

/// <summary>
/// KafkaOptions class, provides a way to configure Kafka.
/// </summary>
public class KafkaOptions
{
    #region Properties
    /// <summary>
    /// get/set - The name of the Elasticsearch index for unpublished content.
    /// </summary>
    public string IndexingTopic { get; set; } = "";

    /// <summary>
    /// get/set - The topic to publish notifications to.
    /// </summary>
    public string NotificationTopic { get; set; } = "";
    #endregion
}
