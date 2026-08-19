namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationRunLogModel class, represents a single engine decision recorded during a v2 run.
/// LLM entries carry the prompt/response and token counts; non-LLM entries record condition
/// evaluations, exclusions, skips, and flushes so every item's outcome has a trace.
/// </summary>
public class AutomationRunLogModel
{
    #region Properties
    /// <summary>
    /// get/set - Identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - The run this entry belongs to.
    /// </summary>
    public long RunId { get; set; }

    /// <summary>
    /// get/set - The step that produced this entry.
    /// </summary>
    public string StepName { get; set; } = "";

    /// <summary>
    /// get/set - The action this entry is for (null for step-level entries).
    /// </summary>
    public string? ActionName { get; set; }

    /// <summary>
    /// get/set - The action type; null for analysis entries.
    /// </summary>
    public string? ActionType { get; set; }

    /// <summary>
    /// get/set - The analysis this entry is for (null for non-analysis entries).
    /// </summary>
    public string? AnalysisName { get; set; }

    /// <summary>
    /// get/set - The content item this entry relates to.
    /// </summary>
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - The attempt number for retried LLM requests.
    /// </summary>
    public int Attempt { get; set; } = 1;

    /// <summary>
    /// get/set - Whether this entry records an LLM exchange.
    /// </summary>
    public bool IsLLM { get; set; }

    /// <summary>
    /// get/set - The comparison variant ('A'/'B') for comparison runs.
    /// </summary>
    public string? Variant { get; set; }

    /// <summary>
    /// get/set - The prompt sent to the LLM.
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// get/set - The LLM response, or a decision description for non-LLM entries.
    /// </summary>
    public string? Response { get; set; }

    /// <summary>
    /// get/set - Prompt token count reported by the LLM.
    /// </summary>
    public int? PromptTokens { get; set; }

    /// <summary>
    /// get/set - Completion token count reported by the LLM.
    /// </summary>
    public int? CompletionTokens { get; set; }

    /// <summary>
    /// get/set - Wall-clock duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// get/set - The entry outcome.
    /// </summary>
    public string Outcome { get; set; } = "info";

    /// <summary>
    /// get/set - JSON detail (condition values, resulting change, exclusion reason).
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// get/set - When the entry was recorded (UTC).
    /// </summary>
    public DateTime CreatedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationRunLogModel.
    /// </summary>
    public AutomationRunLogModel() { }

    /// <summary>
    /// Creates a new instance of an AutomationRunLogModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AutomationRunLogModel(Entities.AutomationRunLog entity)
    {
        this.Id = entity.Id;
        this.RunId = entity.AutomationRunId;
        this.StepName = entity.StepName;
        this.ActionName = entity.ActionName;
        this.ActionType = entity.ActionType;
        this.AnalysisName = entity.AnalysisName;
        this.ContentId = entity.ContentId;
        this.Attempt = entity.Attempt;
        this.IsLLM = entity.IsLLM;
        this.Variant = entity.Variant;
        this.Prompt = entity.Prompt;
        this.Response = entity.Response;
        this.PromptTokens = entity.PromptTokens;
        this.CompletionTokens = entity.CompletionTokens;
        this.DurationMs = entity.DurationMs;
        this.Outcome = entity.Outcome;
        this.Detail = entity.Detail;
        this.CreatedOn = entity.CreatedOn;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new AutomationRunLog entity from this model.
    /// </summary>
    /// <param name="runId"></param>
    /// <returns></returns>
    public Entities.AutomationRunLog ToEntity(long runId)
    {
        return new Entities.AutomationRunLog(runId, this.StepName, this.Outcome)
        {
            ActionName = this.ActionName,
            ActionType = this.ActionType,
            AnalysisName = this.AnalysisName,
            ContentId = this.ContentId,
            Attempt = this.Attempt,
            IsLLM = this.IsLLM,
            Variant = this.Variant,
            Prompt = this.Prompt,
            Response = this.Response,
            PromptTokens = this.PromptTokens,
            CompletionTokens = this.CompletionTokens,
            DurationMs = this.DurationMs,
            Detail = this.Detail,
        };
    }
    #endregion
}
