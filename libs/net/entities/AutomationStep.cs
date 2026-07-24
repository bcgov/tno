using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AutomationStep class, provides a DB model to manage an ordered step within an automation profile.
/// A step has a templated prompt, an execution target timing, an optional filter (gate or enrichment),
/// and an ordered collection of actions.
/// </summary>
[Table("automation_step")]
public class AutomationStep : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the owning automation profile.
    /// </summary>
    [Column("automation_profile_id")]
    public int AutomationProfileId { get; set; }

    /// <summary>
    /// get/set - The owning automation profile.
    /// </summary>
    public virtual AutomationProfile? AutomationProfile { get; set; }

    /// <summary>
    /// get/set - The templated prompt sent to the LLM for this step.
    /// </summary>
    [Column("prompt")]
    public string Prompt { get; set; } = "";

    /// <summary>
    /// get/set - The step execution target timing ('start', 'content', 'end').
    /// </summary>
    [Column("target")]
    public string Target { get; set; } = "content";

    /// <summary>
    /// get/set - Optional foreign key to a filter used to gate the iterated item or enrich with results.
    /// </summary>
    [Column("filter_id")]
    public int? FilterId { get; set; }

    /// <summary>
    /// get/set - The filter for this step.
    /// </summary>
    public Filter? Filter { get; set; }

    /// <summary>
    /// get/set - Optional foreign key to an LLM used for this step's prompts instead of the
    /// profile's LLM.
    /// </summary>
    [Column("llm_id")]
    public int? LLMId { get; set; }

    /// <summary>
    /// get/set - The LLM override for this step.
    /// </summary>
    public LLM? LLM { get; set; }

    /// <summary>
    /// get/set - Whether each action sends its own prompt (step prompt + that action's prompt)
    /// instead of one combined prompt for all actions. Actions still execute sequentially, so an
    /// abort stops later actions before their prompts are ever sent.
    /// </summary>
    [Column("send_separate_prompts")]
    public bool SendSeparatePrompts { get; set; }

    /// <summary>
    /// get/set - Whether the step runs as a chat-completions conversation: the step prompt is the
    /// system prompt, and each action is sent as its own user message in a shared conversation
    /// (the model retains the context of earlier action responses).
    /// </summary>
    [Column("use_chat_completions")]
    public bool UseChatCompletions { get; set; }

    /// <summary>
    /// get/set - Whether the step filter applies to the iterated profile content item (gate)
    /// rather than running as a separate Elasticsearch enrichment query.
    /// </summary>
    [Column("apply_to_automation_filter")]
    public bool ApplyToAutomationFilter { get; set; }

    /// <summary>
    /// get/set - Whether the step iterates over the step filter's results, executing the step
    /// (and applying its actions) once per content item. Only used for 'start' and 'end' targets
    /// with a step filter.
    /// </summary>
    [Column("iterate_step_filter")]
    public bool IterateStepFilter { get; set; }

    /// <summary>
    /// get - The ordered collection of actions within this step (ordered by SortOrder).
    /// </summary>
    public virtual List<AutomationAction> Actions { get; } = new List<AutomationAction>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationStep object.
    /// </summary>
    protected AutomationStep() : base() { }

    /// <summary>
    /// Creates a new instance of an AutomationStep object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="profile"></param>
    public AutomationStep(string name, AutomationProfile profile) : base(name)
    {
        this.AutomationProfile = profile ?? throw new ArgumentNullException(nameof(profile));
        this.AutomationProfileId = profile.Id;
    }

    /// <summary>
    /// Creates a new instance of an AutomationStep object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="profileId"></param>
    public AutomationStep(string name, int profileId) : base(name)
    {
        this.AutomationProfileId = profileId;
    }

    /// <summary>
    /// Creates a new instance of an AutomationStep object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="profileId"></param>
    public AutomationStep(int id, string name, int profileId) : base(id, name)
    {
        this.AutomationProfileId = profileId;
    }
    #endregion

    #region Methods
    public bool Equals(AutomationStep? other)
    {
        if (other == null) return false;
        return this.Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as AutomationStep);
    public override int GetHashCode() => this.Id.GetHashCode();
    #endregion
}
