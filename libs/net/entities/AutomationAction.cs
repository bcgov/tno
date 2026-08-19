using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AutomationAction class, provides a DB model to manage an ordered action within an automation step.
/// An action executes only when the configured confirmation statement is present in the LLM response,
/// and is bounded by an optional maximum number of executions per run.
/// Actions are ordered within their step by SortOrder.
/// </summary>
[Table("automation_action")]
public class AutomationAction : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// get/set - A name to identify the action.
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to the owning automation step.
    /// </summary>
    [Column("automation_step_id")]
    public int AutomationStepId { get; set; }

    /// <summary>
    /// get/set - The owning automation step.
    /// </summary>
    public virtual AutomationStep? AutomationStep { get; set; }

    /// <summary>
    /// get/set - The templated prompt sent to the LLM for this action.
    /// </summary>
    [Column("prompt")]
    public string Prompt { get; set; } = "";

    /// <summary>
    /// get/set - The action type key that identifies the registered handler to execute.
    /// </summary>
    [Column("action_type")]
    public string ActionType { get; set; } = "";

    /// <summary>
    /// get/set - The maximum number of times this action may execute in a single profile run.
    /// When null the action is unbounded.
    /// </summary>
    [Column("max_calls")]
    public int? MaxCalls { get; set; }

    /// <summary>
    /// get/set - The confirmation statement that must be present in the LLM response for the action to execute.
    /// </summary>
    [Column("confirmation_statement")]
    public string ConfirmationStatement { get; set; } = "";

    /// <summary>
    /// get/set - The content field to update. Only used when ActionType is 'update-content-field'
    /// (e.g. 'headline', 'byline', 'summary', 'body', 'edition', 'section', 'page').
    /// </summary>
    [Column("content_field")]
    public string? ContentField { get; set; }

    /// <summary>
    /// get/set - Foreign key to the content action to add. Used when ActionType is 'add-action' or 'select-top'.
    /// </summary>
    [Column("content_action_id")]
    public int? ContentActionId { get; set; }

    /// <summary>
    /// get/set - The scoring objective key (e.g. 'top-story', 'featured', 'commentary').
    /// Links 'score-content' actions to the 'select-top' action that consumes their scores.
    /// </summary>
    [Column("objective")]
    public string? Objective { get; set; }

    /// <summary>
    /// get/set - The content action to add.
    /// </summary>
    public Action? ContentAction { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report to run. Only used when ActionType is 'run-report'.
    /// </summary>
    [Column("report_id")]
    public int? ReportId { get; set; }

    /// <summary>
    /// get/set - The report to run.
    /// </summary>
    public Report? Report { get; set; }

    /// <summary>
    /// get/set - Foreign key to the notification to run. Only used when ActionType is 'run-notification'.
    /// </summary>
    [Column("notification_id")]
    public int? NotificationId { get; set; }

    /// <summary>
    /// get/set - The notification to run.
    /// </summary>
    public Notification? Notification { get; set; }

    /// <summary>
    /// get/set - Foreign key to the filter that produces this action's content collection.
    /// Only used when ActionType is 'fetch-content'; the filter's Elasticsearch query is executed
    /// once per run and the results are made available to later actions that reference this one.
    /// </summary>
    [Column("filter_id")]
    public int? FilterId { get; set; }

    /// <summary>
    /// get/set - The filter that produces this action's content collection.
    /// </summary>
    public Filter? Filter { get; set; }

    /// <summary>
    /// get/set - Foreign key to a prior automation action (earlier in this step or in an earlier step).
    /// Only used when ActionType is 'deduplicate'; the content items the prior action successfully
    /// processed - or, when it is a 'fetch-content' action, the collection it fetched - are compared
    /// against the current item to detect duplicates.
    /// </summary>
    [Column("prior_action_id")]
    public int? PriorActionId { get; set; }

    /// <summary>
    /// get/set - The prior automation action to compare against.
    /// </summary>
    public AutomationAction? PriorAction { get; set; }

    /// <summary>
    /// get/set - Optional foreign key to an LLM used for this action's prompt instead of the
    /// step/profile LLM. Only used when the step sends separate prompts per action.
    /// </summary>
    [Column("llm_id")]
    public int? LLMId { get; set; }

    /// <summary>
    /// get/set - The LLM override for this action.
    /// </summary>
    public LLM? LLM { get; set; }

    /// <summary>
    /// get/set - Whether the action executes unconditionally, without LLM confirmation.
    /// The action's prompt is excluded from the composed step prompt and no confirmation
    /// statement is required. Only meaningful for action types that do not extract a value.
    /// </summary>
    [Column("auto_execute")]
    public bool AutoExecute { get; set; }

    /// <summary>
    /// get/set - Whether to abort the remaining actions on the step when this action does not
    /// receive its confirmation. Lets an action like 'publish' stop further actions if the content
    /// was not published. Only meaningful for actions that require a confirmation (not AutoExecute).
    /// </summary>
    [Column("abort_if_no_confirmation")]
    public bool AbortIfNoConfirmation { get; set; }

    /// <summary>
    /// get/set - Which content the action operates on. Null (or "original") targets the iterated
    /// content item; otherwise the identifier of a content item created earlier in the same step
    /// by a 'create-content' action. Only honoured when the step sends separate prompts per action.
    /// </summary>
    [Column("works_on")]
    public string? WorksOn { get; set; }

    /// <summary>
    /// get/set - For 'create-content' actions, the identifier later actions in the same step use
    /// (via WorksOn) to reference the content item this action creates (e.g. "c1").
    /// </summary>
    [Column("create_identifier")]
    public string? CreateIdentifier { get; set; }

    /// <summary>
    /// get/set - For 'create-content' actions, whether to clone the iterated content item's fields
    /// as the starting point for the new item (before applying extracted data and prompt values).
    /// </summary>
    [Column("create_clone")]
    public bool CreateClone { get; set; }

    /// <summary>
    /// get/set - Action-type specific configuration stored as JSON (e.g. 'extract-data' key map and
    /// custom pairs; 'create-content' static field overrides such as the headline).
    /// </summary>
    [Column("settings")]
    public System.Text.Json.JsonDocument Settings { get; set; } = System.Text.Json.JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Whether the action is enabled.
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - A way to control the execution order of actions within a step.
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationAction object.
    /// </summary>
    public AutomationAction() { }

    /// <summary>
    /// Creates a new instance of an AutomationAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="step"></param>
    /// <param name="actionType"></param>
    public AutomationAction(AutomationStep step, string actionType)
    {
        this.AutomationStep = step ?? throw new ArgumentNullException(nameof(step));
        this.AutomationStepId = step.Id;
        this.ActionType = actionType;
    }

    /// <summary>
    /// Creates a new instance of an AutomationAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="stepId"></param>
    /// <param name="actionType"></param>
    public AutomationAction(int stepId, string actionType)
    {
        this.AutomationStepId = stepId;
        this.ActionType = actionType;
    }
    #endregion

    #region Methods
    public bool Equals(AutomationAction? other)
    {
        if (other == null) return false;
        return this.Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as AutomationAction);
    public override int GetHashCode() => this.Id.GetHashCode();
    #endregion
}
