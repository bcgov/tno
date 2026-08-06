using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AutomationRunResponse class, records a single LLM prompt/response captured during an automation run.
/// Stored in its own table (many-to-one to the run) so the large prompt/response text is kept out of the
/// run's Summary field - this keeps run data small and reduces the automation service's memory footprint.
/// </summary>
[Table("automation_run_response")]
public class AutomationRunResponse : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the run this response belongs to.
    /// </summary>
    [Column("automation_run_id")]
    public long AutomationRunId { get; set; }

    /// <summary>
    /// get/set - The run this response belongs to.
    /// </summary>
    public virtual AutomationRun? AutomationRun { get; set; }

    /// <summary>
    /// get/set - The step that produced this response.
    /// </summary>
    [Column("step_id")]
    public int StepId { get; set; }

    /// <summary>
    /// get/set - The name of the step that produced this response.
    /// </summary>
    [Column("step_name")]
    public string StepName { get; set; } = "";

    /// <summary>
    /// get/set - The name of the action this response is for (null for a shared step prompt).
    /// </summary>
    [Column("action_name")]
    public string? ActionName { get; set; }

    /// <summary>
    /// get/set - The content item this response relates to (null for non-content steps).
    /// </summary>
    [Column("content_id")]
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - The prompt sent to the LLM (only recorded when prompt capture is enabled).
    /// </summary>
    [Column("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// get/set - The LLM response text.
    /// </summary>
    [Column("response")]
    public string Response { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationRunResponse.
    /// </summary>
    public AutomationRunResponse() { }

    /// <summary>
    /// Creates a new instance of an AutomationRunResponse, initializes with specified parameters.
    /// </summary>
    public AutomationRunResponse(long automationRunId, int stepId, string stepName, string? actionName, long? contentId, string? prompt, string response)
    {
        this.AutomationRunId = automationRunId;
        this.StepId = stepId;
        this.StepName = stepName;
        this.ActionName = actionName;
        this.ContentId = contentId;
        this.Prompt = prompt;
        this.Response = response;
    }
    #endregion
}
