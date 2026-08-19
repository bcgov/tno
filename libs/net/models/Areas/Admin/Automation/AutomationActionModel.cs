namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationActionModel class, represents a step action.
/// Actions have no name; they are ordered within their step by array order.
/// </summary>
public class AutomationActionModel
{
    #region Properties
    /// <summary>
    /// get/set - Action identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Action name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Action prompt.
    /// </summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// get/set - Action type identifier.
    /// </summary>
    public string ActionType { get; set; } = "";

    /// <summary>
    /// get/set - Maximum number of times this action can be performed per run.
    /// </summary>
    public int? MaxCalls { get; set; }

    /// <summary>
    /// get/set - Output confirmation statement required before action is performed.
    /// </summary>
    public string ConfirmationStatement { get; set; } = "";

    /// <summary>
    /// get/set - The content field to update; only used when ActionType is 'update-content-field'.
    /// </summary>
    public string? ContentField { get; set; }

    /// <summary>
    /// get/set - Foreign key to the content action to add; used when ActionType is 'add-action' or 'select-top'.
    /// </summary>
    public int? ContentActionId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report to run; only used when ActionType is 'run-report'.
    /// </summary>
    public int? ReportId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the notification to run; only used when ActionType is 'run-notification'.
    /// </summary>
    public int? NotificationId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the filter that produces this action's content collection;
    /// only used when ActionType is 'fetch-content'.
    /// </summary>
    public int? FilterId { get; set; }

    /// <summary>
    /// get/set - The filter's Elasticsearch query, so the automation service does not need to look
    /// the filter up.
    /// </summary>
    public string? FilterQuery { get; set; }

    /// <summary>
    /// get/set - The filter's settings, which determine the index the query is executed against.
    /// </summary>
    public string? FilterSettings { get; set; }

    /// <summary>
    /// get/set - Foreign key to a prior automation action whose processed content - or fetched
    /// collection - is compared against the current item; only used when ActionType is 'deduplicate'.
    /// </summary>
    public int? PriorActionId { get; set; }

    /// <summary>
    /// get/set - The scoring objective key; links 'score-content' actions to their 'select-top' action.
    /// </summary>
    public string? Objective { get; set; }

    /// <summary>
    /// get/set - Optional LLM used for this action's prompt instead of the step/profile LLM;
    /// only used when the step sends separate prompts per action.
    /// </summary>
    public int? LLMId { get; set; }

    /// <summary>
    /// get/set - Whether the action executes unconditionally, without LLM confirmation;
    /// only used by action types that do not extract a value.
    /// </summary>
    public bool AutoExecute { get; set; }

    /// <summary>
    /// get/set - Whether to abort the remaining actions on the step when this action does not
    /// receive its confirmation (e.g. a 'publish' that did not happen). Only meaningful for actions
    /// that require a confirmation (not AutoExecute).
    /// </summary>
    public bool AbortIfNoConfirmation { get; set; }

    /// <summary>
    /// get/set - Which content the action operates on. Null (or "original") targets the iterated
    /// content item; otherwise the identifier of a content item created earlier in the same step by
    /// a 'create-content' action. Only honoured when the step sends separate prompts per action.
    /// </summary>
    public string? WorksOn { get; set; }

    /// <summary>
    /// get/set - For 'create-content' actions, the identifier later actions reference (via WorksOn).
    /// </summary>
    public string? CreateIdentifier { get; set; }

    /// <summary>
    /// get/set - For 'create-content' actions, whether to clone the iterated content item as the
    /// starting point for the new item before applying extracted data and prompt values.
    /// </summary>
    public bool CreateClone { get; set; }

    /// <summary>
    /// get/set - Action-type specific configuration stored as JSON (e.g. 'extract-data' key map and
    /// custom pairs; 'create-content' static field overrides such as the headline).
    /// </summary>
    public System.Text.Json.JsonDocument Settings { get; set; } = System.Text.Json.JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Whether action is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationActionModel.
    /// </summary>
    public AutomationActionModel() { }

    /// <summary>
    /// Creates a new instance of an AutomationActionModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AutomationActionModel(Entities.AutomationAction entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Prompt = entity.Prompt;
        this.ActionType = entity.ActionType;
        this.MaxCalls = entity.MaxCalls;
        this.ConfirmationStatement = entity.ConfirmationStatement;
        this.ContentField = entity.ContentField;
        this.ContentActionId = entity.ContentActionId;
        this.ReportId = entity.ReportId;
        this.NotificationId = entity.NotificationId;
        this.FilterId = entity.FilterId;
        this.FilterQuery = entity.Filter?.Query.RootElement.GetRawText();
        this.FilterSettings = entity.Filter?.Settings.RootElement.GetRawText();
        this.PriorActionId = entity.PriorActionId;
        this.Objective = entity.Objective;
        this.LLMId = entity.LLMId;
        this.AutoExecute = entity.AutoExecute;
        this.AbortIfNoConfirmation = entity.AbortIfNoConfirmation;
        this.WorksOn = entity.WorksOn;
        this.CreateIdentifier = entity.CreateIdentifier;
        this.CreateClone = entity.CreateClone;
        this.Settings = entity.Settings;
        this.IsEnabled = entity.IsEnabled;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new AutomationAction entity from this model.
    /// </summary>
    /// <param name="stepId"></param>
    /// <param name="sortOrder"></param>
    /// <returns></returns>
    public Entities.AutomationAction ToEntity(int stepId, int sortOrder)
    {
        return new Entities.AutomationAction
        {
            Id = this.Id,
            Name = this.Name,
            AutomationStepId = stepId,
            Prompt = this.Prompt,
            ActionType = this.ActionType,
            MaxCalls = this.MaxCalls,
            ConfirmationStatement = this.ConfirmationStatement,
            ContentField = this.ContentField,
            ContentActionId = this.ContentActionId,
            ReportId = this.ReportId,
            NotificationId = this.NotificationId,
            FilterId = this.FilterId,
            PriorActionId = this.PriorActionId,
            Objective = this.Objective,
            LLMId = this.LLMId,
            AutoExecute = this.AutoExecute,
            AbortIfNoConfirmation = this.AbortIfNoConfirmation,
            WorksOn = this.WorksOn,
            CreateIdentifier = this.CreateIdentifier,
            CreateClone = this.CreateClone,
            Settings = this.Settings ?? System.Text.Json.JsonDocument.Parse("{}"),
            IsEnabled = this.IsEnabled,
            SortOrder = sortOrder,
        };
    }
    #endregion
}
