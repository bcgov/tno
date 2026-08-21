using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AutomationRunLog class, records a single engine decision captured during an automation run.
/// Unlike <see cref="AutomationRunResponse"/> (v1 prompt/response records), a log entry is written
/// for every decision the v2 engine makes - LLM analyses, property-condition gates, exclusions,
/// skips, and flushes - so every item in a run resolves to a trace explaining its outcome.
/// Prompts are always recorded. Entries are retained for the current date only and pruned daily.
/// </summary>
[Table("automation_run_log")]
public class AutomationRunLog : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the run this entry belongs to.
    /// </summary>
    [Column("automation_run_id")]
    public long AutomationRunId { get; set; }

    /// <summary>
    /// get/set - The run this entry belongs to.
    /// </summary>
    public virtual AutomationRun? AutomationRun { get; set; }

    /// <summary>
    /// get/set - The name of the step that produced this entry.
    /// </summary>
    [Column("step_name")]
    public string StepName { get; set; } = "";

    /// <summary>
    /// get/set - The name of the action this entry is for (null for step-level entries).
    /// </summary>
    [Column("action_name")]
    public string? ActionName { get; set; }

    /// <summary>
    /// get/set - The action type (e.g. 'content.publish', 'exclude'); null for analysis entries.
    /// </summary>
    [Column("action_type")]
    public string? ActionType { get; set; }

    /// <summary>
    /// get/set - The name of the analysis this entry is for (null for non-analysis entries).
    /// </summary>
    [Column("analysis_name")]
    public string? AnalysisName { get; set; }

    /// <summary>
    /// get/set - The content item this entry relates to (null for run/step-level entries).
    /// </summary>
    [Column("content_id")]
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - The attempt number for retried LLM requests (1 for the first attempt).
    /// </summary>
    [Column("attempt")]
    public int Attempt { get; set; } = 1;

    /// <summary>
    /// get/set - Whether this entry records an LLM exchange (true) or an engine decision (false).
    /// Non-LLM entries carry no token cost and are rendered distinctly in the log viewer.
    /// </summary>
    [Column("is_llm")]
    public bool IsLLM { get; set; }

    /// <summary>
    /// get/set - The comparison variant this entry belongs to ('A' or 'B') when the run executed
    /// in comparison mode; null for normal runs.
    /// </summary>
    [Column("variant")]
    public string? Variant { get; set; }

    /// <summary>
    /// get/set - The prompt sent to the LLM. Always recorded for LLM entries (no capture flag).
    /// </summary>
    [Column("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// get/set - The LLM response text, or a decision description for non-LLM entries.
    /// </summary>
    [Column("response")]
    public string? Response { get; set; }

    /// <summary>
    /// get/set - Prompt token count reported by the LLM (null when unavailable or non-LLM).
    /// </summary>
    [Column("prompt_tokens")]
    public int? PromptTokens { get; set; }

    /// <summary>
    /// get/set - Completion token count reported by the LLM (null when unavailable or non-LLM).
    /// </summary>
    [Column("completion_tokens")]
    public int? CompletionTokens { get; set; }

    /// <summary>
    /// get/set - Wall-clock duration of the exchange or decision in milliseconds.
    /// </summary>
    [Column("duration_ms")]
    public long DurationMs { get; set; }

    /// <summary>
    /// get/set - The outcome of the entry: 'confirmed', 'not-confirmed', 'condition-failed',
    /// 'condition-passed', 'executed', 'skipped', 'excluded', 'aborted', 'failed', 'flushed',
    /// 'explain', or 'info'.
    /// </summary>
    [Column("outcome")]
    public string Outcome { get; set; } = "info";

    /// <summary>
    /// get/set - JSON detail for the entry: evaluated condition values, the resulting change,
    /// an exclusion reason, or truncation flags. Kept small; large text belongs in Prompt/Response.
    /// </summary>
    [Column("detail")]
    public string? Detail { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationRunLog.
    /// </summary>
    public AutomationRunLog() { }

    /// <summary>
    /// Creates a new instance of an AutomationRunLog, initializes with specified parameters.
    /// </summary>
    /// <param name="automationRunId"></param>
    /// <param name="stepName"></param>
    /// <param name="outcome"></param>
    public AutomationRunLog(long automationRunId, string stepName, string outcome)
    {
        this.AutomationRunId = automationRunId;
        this.StepName = stepName;
        this.Outcome = outcome;
    }
    #endregion
}
