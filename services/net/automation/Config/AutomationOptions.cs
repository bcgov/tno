using TNO.Services.Config;

namespace TNO.Services.Automation.Config;

/// <summary>
/// AutomationOptions class, configuration options for automation workflows.
/// </summary>
public class AutomationOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - Optional fallback time zone for profile schedules.
    /// </summary>
    public string DefaultTimeZone { get; set; } = "Pacific Standard Time";

    /// <summary>
    /// get/set - Minutes a Running run may go without posting a response record (its activity
    /// heartbeat) before the reconciliation sweep marks it Failed as abandoned (the executing
    /// instance likely crashed). A healthy run posts responses as it completes each step, so this
    /// should exceed the longest expected gap between steps. Set to zero or less to disable.
    /// </summary>
    public int AbandonedRunInactivityMinutes { get; set; } = 30;

    /// <summary>
    /// get/set - The number of days to retain automation run history before pruning. Runs older than
    /// this are deleted on a periodic sweep. Set to zero or less to disable pruning.
    /// </summary>
    public int RunRetentionDays { get; set; } = 7;

    /// <summary>
    /// get/set - The number of days of decision-log entries to keep (1 = the current date only,
    /// the default). The log records every prompt, response, and engine decision, so its retention
    /// is deliberately independent of (and much shorter than) the run-history retention.
    /// Set to zero or less to disable pruning.
    /// </summary>
    public int RunLogRetentionDays { get; set; } = 1;

    /// <summary>
    /// get/set - The default tone pool used when applying sentiment values to content.
    /// </summary>
    public int DefaultTonePoolId { get; set; } = 1;

    /// <summary>
    /// get/set - How many content items a step processes concurrently. Steps always execute in
    /// sequence; this parallelizes the per-item LLM calls and content updates within a step.
    /// Bound by the LLM endpoint's rate limits (throttled requests are retried).
    /// </summary>
    public int MaxParallelContentItems { get; set; } = 8;

    /// <summary>
    /// get/set - Whether the run summary records the prompt sent for each LLM response.
    /// Useful for debugging prompt composition; disabled by default because prompts embed the
    /// full content payload, which makes run summaries large.
    /// </summary>
    public bool IncludeLLMPromptsInSummary { get; set; }

    /// <summary>
    /// get/set - Seconds before an LLM request times out (minimum 30).
    /// </summary>
    public int LLMRequestTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// get/set - Total attempts for an LLM request; timeouts, throttling (429), connection
    /// failures, and server errors are retried until the attempts are exhausted.
    /// </summary>
    public int LLMRequestAttempts { get; set; } = 3;

    /// <summary>
    /// get/set - A comma separated list of Kafka topics to consume automation requests from.
    /// </summary>
    public string Topics { get; set; } = "automation";

    /// <summary>
    /// get/set - Minutes a queued run must be waiting before the reconciliation sweep executes it.
    /// Recovers runs whose Kafka message was lost without racing fresh deliveries.
    /// </summary>
    public int StalePendingRunMinutes { get; set; } = 5;
    #endregion

    #region Methods
    /// <summary>
    /// Get the configured topics, or return the default topics.
    /// </summary>
    /// <param name="defaultTopics"></param>
    /// <returns></returns>
    public string[] GetTopics(string[]? defaultTopics = null)
    {
        var topics = this.Topics?.Split(',').Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToArray() ?? Array.Empty<string>();
        return topics.Length > 0 ? topics : defaultTopics ?? Array.Empty<string>();
    }
    #endregion
}
