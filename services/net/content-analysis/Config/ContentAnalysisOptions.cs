using TNO.Services.Config;

namespace TNO.Services.ContentAnalysis.Config;

/// <summary>
/// ContentAnalysisOptions class, configuration for the Content-Analysis service.
/// </summary>
public class ContentAnalysisOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - Comma-separated Kafka topics of analysis requests for added and changed content
    /// (and editor and administrator requests). Empty consumes none, e.g. for a backfill-only instance.
    /// </summary>
    public string Topics { get; set; } = "analysis";

    /// <summary>
    /// get/set - The Kafka topic backfills send content to. It is consumed apart from 'Topics', so a
    /// backfill never delays new content. Empty consumes none.
    /// </summary>
    public string BackfillTopic { get; set; } = "analysis-backfill";

    /// <summary>
    /// get/set - The Kafka topic a failed request is sent to for a delayed retry; also consumed.
    /// Empty sends failures straight to the dead-letter topic.
    /// </summary>
    public string RetryTopic { get; set; } = "analysis-retry";

    /// <summary>
    /// get/set - The Kafka topic a request is sent to when its retries are exhausted or its failure
    /// is permanent. Not consumed; replay failures from the administration page.
    /// </summary>
    public string DeadLetterTopic { get; set; } = "analysis-dlq";

    /// <summary>
    /// get/set - Comma-separated processes this service runs, each producing its part of the
    /// analysis and applying it to the content's empty field: Metadata (key facts, people and
    /// organizations, places, events, topics), Summary, Quotes, Tags, Contributor, Topics.
    /// Empty runs nothing, and no topic is consumed.
    /// </summary>
    public string Processes { get; set; } = "Metadata,Summary,Quotes,Tags,Contributor,Topics";

    /// <summary>
    /// get/set - Comma-separated request reasons this service analyzes: Lifecycle (content added or
    /// changed), Reanalysis (an editor's request), Backfill, Replay. A request for any other reason
    /// is consumed and committed without being analyzed or recorded, so its topic does not lag, e.g.
    /// to analyze only what editors and administrators ask for in a test environment.
    /// </summary>
    public string Reasons { get; set; } = "Lifecycle,Reanalysis,Backfill,Replay";

    /// <summary>
    /// get/set - Seconds after a content change before it is analyzed, so a burst of edits is
    /// analyzed once (applies to lifecycle requests).
    /// </summary>
    public int QuietPeriodSeconds { get; set; } = 120;

    /// <summary>
    /// get/set - Attempts made for a request before it is sent to the dead-letter topic.
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// get/set - Seconds before the first retry; each later retry waits twice as long.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 30;

    /// <summary>
    /// get/set - The longest wait before a retry, in seconds.
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 3600;

    /// <summary>
    /// get/set - Seconds between refreshes of the runtime settings (LLM, exclusions).
    /// </summary>
    public int SettingsRefreshSeconds { get; set; } = 60;

    /// <summary>
    /// get/set - Seconds a backfill work order's status is cached, so a cancelled backfill's requests
    /// are skipped.
    /// </summary>
    public int WorkOrderRefreshSeconds { get; set; } = 30;

    /// <summary>
    /// get/set - The share of the LLM's rate limits backfill requests may use.
    /// </summary>
    public double BackfillShare { get; set; } = 0.2;

    /// <summary>
    /// get/set - Per-attempt LLM request timeout in seconds.
    /// </summary>
    public int LLMRequestTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// get/set - Attempts per LLM request for throttling and transient failures.
    /// </summary>
    public int LLMRequestAttempts { get; set; } = 3;

    /// <summary>
    /// get/set - Percentage of the context window held back for token estimation error.
    /// </summary>
    public int SafetyMarginPercent { get; set; } = 10;

    /// <summary>
    /// get/set - Tokens of context repeated between chunks of a long story.
    /// </summary>
    public int OverlapTokens { get; set; } = 150;

    /// <summary>
    /// get/set - Text shorter than this is not usable; only metadata is recorded.
    /// </summary>
    public int MinTextCharacters { get; set; } = 40;
    #endregion

    #region Methods
    /// <summary>
    /// The topics of analysis requests for added and changed content.
    /// </summary>
    /// <returns></returns>
    public string[] GetTopics() => this.Topics.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// The configured processes.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">A process name is not recognized.</exception>
    public TNO.Entities.AnalysisProcess GetProcesses()
    {
        var processes = TNO.Entities.AnalysisProcess.None;
        foreach (var name in this.Processes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Enum.TryParse<TNO.Entities.AnalysisProcess>(name, true, out var process) || process == TNO.Entities.AnalysisProcess.None)
                throw new InvalidOperationException($"Content-Analysis process '{name}' is not recognized. Use Metadata, Summary, Quotes, Tags, Contributor, or Topics.");
            processes |= process;
        }
        return processes;
    }

    /// <summary>
    /// The configured request reasons.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">A reason is not recognized.</exception>
    public TNO.Entities.AnalysisRequestReason[] GetReasons()
    {
        var reasons = new List<TNO.Entities.AnalysisRequestReason>();
        foreach (var name in this.Reasons.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Enum.TryParse<TNO.Entities.AnalysisRequestReason>(name, true, out var reason) || !Enum.IsDefined(reason))
                throw new InvalidOperationException($"Content-Analysis request reason '{name}' is not recognized. Use Lifecycle, Reanalysis, Backfill, or Replay.");
            reasons.Add(reason);
        }
        return reasons.Distinct().ToArray();
    }
    #endregion
}
