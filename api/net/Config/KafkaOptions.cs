namespace TNO.API.Config;

/// <summary>
/// KafkaOptions class, provides a way to configure Kafka.
/// </summary>
public class KafkaOptions
{
    #region Properties
    /// <summary>
    /// get/set - The Kafka topic name to request indexing content in Elasticsearch.
    /// </summary>
    public string IndexingTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name to request transcripts.
    /// </summary>
    public string TranscriptionTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name to request NLP.
    /// </summary>
    public string NLPTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name to request a remote file.
    /// </summary>
    public string FileRequestTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name to request a notification to be sent.
    /// </summary>
    public string NotificationTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name to request a report to be sent.
    /// </summary>
    public string ReportingTopic { get; set; } = "";
    #endregion
}
