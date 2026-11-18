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
    /// get/set - The Kafka topic name to request auto clips.
    /// </summary>
    public string AutoClipTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name to request analysis of added or changed content (and editor
    /// and administrator requests).
    /// </summary>
    public string AnalysisTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name a backfill sends content to for analysis.
    /// </summary>
    public string AnalysisBackfillTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name Content-Analysis sends failed requests to for a delayed retry.
    /// </summary>
    public string AnalysisRetryTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name Content-Analysis sends requests to when retries are exhausted.
    /// </summary>
    public string AnalysisDeadLetterTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Content-Analysis consumer group, to report how many requests are waiting.
    /// </summary>
    public string AnalysisConsumerGroup { get; set; } = "ContentAnalysis";

    /// <summary>
    /// get/set - The Kafka topic name the Event Handler receives work orders on.
    /// </summary>
    public string WorkOrderTopic { get; set; } = "";

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

    /// <summary>
    /// get/set - The Kafka topic name to request a scheduled event to be executed.
    /// </summary>
    public string EventTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name to request FFmpeg processes.
    /// </summary>
    public string FFmpegTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name to add content to folders.
    /// </summary>
    public string FolderTopic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic name to request an automation run be executed.
    /// </summary>
    public string AutomationTopic { get; set; } = "automation";
    #endregion
}
