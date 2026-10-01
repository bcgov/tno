using TNO.Services.Config;

namespace TNO.Services.ContentAnalysis.Config;

/// <summary>
/// ContentAnalysisOptions class, configuration for the Content-Analysis service.
/// </summary>
public class ContentAnalysisOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - Comma-separated Kafka topics that wake the workers.
    /// </summary>
    public string Topics { get; set; } = "analysis";

    /// <summary>
    /// get/set - Comma-separated processes this service runs, each producing its part of the
    /// analysis and applying it to the content's empty field: Metadata (key facts, people and
    /// organizations, places, events, topics), Summary, Quotes, Tags, Contributor, Topics.
    /// Empty runs nothing.
    /// </summary>
    public string Processes { get; set; } = "Metadata,Summary,Quotes,Tags,Contributor,Topics";

    /// <summary>
    /// get/set - Content items analyzed at once by this instance.
    /// </summary>
    public int MaxConcurrency { get; set; } = 4;

    /// <summary>
    /// get/set - Seconds between polls of the job table when no wake message arrives.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// get/set - Seconds between refreshes of the runtime settings (LLM, exclusions).
    /// </summary>
    public int SettingsRefreshSeconds { get; set; } = 60;

    /// <summary>
    /// get/set - The share of the LLM's rate limits and of this instance's concurrency backfill may use.
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
    /// The topics to subscribe to.
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
    #endregion
}
