namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationRunResponseModel class, a single LLM prompt/response captured during an automation run.
/// </summary>
public class AutomationRunResponseModel
{
    #region Properties
    /// <summary>
    /// get/set - Response identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - The run this response belongs to.
    /// </summary>
    public long AutomationRunId { get; set; }

    /// <summary>
    /// get/set - The step that produced this response.
    /// </summary>
    public int StepId { get; set; }

    /// <summary>
    /// get/set - The name of the step that produced this response.
    /// </summary>
    public string StepName { get; set; } = "";

    /// <summary>
    /// get/set - The name of the action this response is for.
    /// </summary>
    public string? ActionName { get; set; }

    /// <summary>
    /// get/set - The content item this response relates to.
    /// </summary>
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - The prompt sent to the LLM (only present when prompt capture is enabled).
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// get/set - The LLM response text.
    /// </summary>
    public string Response { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationRunResponseModel.
    /// </summary>
    public AutomationRunResponseModel() { }

    /// <summary>
    /// Creates a new instance of an AutomationRunResponseModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AutomationRunResponseModel(Entities.AutomationRunResponse entity)
    {
        this.Id = entity.Id;
        this.AutomationRunId = entity.AutomationRunId;
        this.StepId = entity.StepId;
        this.StepName = entity.StepName;
        this.ActionName = entity.ActionName;
        this.ContentId = entity.ContentId;
        this.Prompt = entity.Prompt;
        this.Response = entity.Response;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new AutomationRunResponse entity from this model for the specified run.
    /// </summary>
    /// <param name="runId"></param>
    /// <returns></returns>
    public Entities.AutomationRunResponse ToEntity(long runId)
    {
        return new Entities.AutomationRunResponse(runId, this.StepId, this.StepName, this.ActionName, this.ContentId, this.Prompt, this.Response);
    }
    #endregion
}
