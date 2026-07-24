namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationStepModel class, represents an ordered automation step.
/// </summary>
public class AutomationStepModel
{
    #region Properties
    /// <summary>
    /// get/set - Step identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Step name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Step description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Step prompt.
    /// </summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// get/set - Step priority; lower values execute first. Persisted as the entity sort order.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// get/set - Step target scope. Supported values: 'content', 'start', 'end'.
    /// </summary>
    public string Target { get; set; } = "content";

    /// <summary>
    /// get/set - Optional filter identifier.
    /// </summary>
    public int? FilterId { get; set; }

    /// <summary>
    /// get - The Elasticsearch query of the step filter (read-only; provided for the automation service).
    /// </summary>
    public string? FilterQuery { get; set; }

    /// <summary>
    /// get - The settings of the step filter (read-only; provided for the automation service).
    /// </summary>
    public string? FilterSettings { get; set; }

    /// <summary>
    /// get/set - Whether the step filter is applied to the iterated profile content item.
    /// </summary>
    public bool ApplyToAutomationFilter { get; set; }

    /// <summary>
    /// get/set - Whether the step iterates over the step filter's results ('start'/'end' targets).
    /// </summary>
    public bool IterateStepFilter { get; set; }

    /// <summary>
    /// get/set - Optional LLM used for this step's prompts instead of the profile's LLM.
    /// </summary>
    public int? LLMId { get; set; }

    /// <summary>
    /// get/set - Whether each action sends its own prompt (step prompt + that action's prompt).
    /// </summary>
    public bool SendSeparatePrompts { get; set; }

    /// <summary>
    /// get/set - Whether the step runs as a chat-completions conversation (step prompt = system
    /// prompt; each action a user message sharing the conversation context).
    /// </summary>
    public bool UseChatCompletions { get; set; }

    /// <summary>
    /// get/set - Whether step is active.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - Action collection.
    /// </summary>
    public IEnumerable<AutomationActionModel> Actions { get; set; } = Array.Empty<AutomationActionModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationStepModel.
    /// </summary>
    public AutomationStepModel() { }

    /// <summary>
    /// Creates a new instance of an AutomationStepModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AutomationStepModel(Entities.AutomationStep entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.Prompt = entity.Prompt;
        this.Priority = entity.SortOrder;
        this.Target = entity.Target;
        this.FilterId = entity.FilterId;
        this.FilterQuery = entity.Filter?.Query.RootElement.GetRawText();
        this.FilterSettings = entity.Filter?.Settings.RootElement.GetRawText();
        this.ApplyToAutomationFilter = entity.ApplyToAutomationFilter;
        this.IterateStepFilter = entity.IterateStepFilter;
        this.LLMId = entity.LLMId;
        this.SendSeparatePrompts = entity.SendSeparatePrompts;
        this.UseChatCompletions = entity.UseChatCompletions;
        this.IsEnabled = entity.IsEnabled;
        this.Actions = entity.Actions
            .OrderBy(a => a.SortOrder)
            .Select(a => new AutomationActionModel(a))
            .ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new AutomationStep entity from this model, including its actions.
    /// </summary>
    /// <param name="profileId"></param>
    /// <returns></returns>
    public Entities.AutomationStep ToEntity(int profileId)
    {
        var step = new Entities.AutomationStep(this.Id, this.Name, profileId)
        {
            Description = this.Description,
            Prompt = this.Prompt,
            SortOrder = this.Priority,
            Target = this.Target,
            FilterId = this.FilterId,
            ApplyToAutomationFilter = this.ApplyToAutomationFilter,
            IterateStepFilter = this.IterateStepFilter,
            LLMId = this.LLMId,
            SendSeparatePrompts = this.SendSeparatePrompts,
            UseChatCompletions = this.UseChatCompletions,
            IsEnabled = this.IsEnabled,
        };

        var sortOrder = 0;
        foreach (var action in this.Actions)
        {
            step.Actions.Add(action.ToEntity(this.Id, sortOrder++));
        }

        return step;
    }
    #endregion
}
