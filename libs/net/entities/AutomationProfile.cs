using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// AutomationProfile class, provides a DB model to manage a named automation configuration profile.
/// A profile optionally selects a filter to load Elasticsearch content for iteration, and owns an
/// ordered collection of steps.
/// </summary>
[Cache("automation_profile")]
[Table("automation_profile")]
public class AutomationProfile : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The schema version of the profile configuration; supports future step/action extensibility.
    /// </summary>
    [Column("schema_version")]
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// get/set - Optional foreign key to the Elasticsearch filter used to select content for iteration.
    /// </summary>
    [Column("filter_id")]
    public int? FilterId { get; set; }

    /// <summary>
    /// get/set - The filter used to select content for iteration.
    /// </summary>
    public Filter? Filter { get; set; }

    /// <summary>
    /// get/set - Optional foreign key to the LLM used to evaluate step/action prompts.
    /// </summary>
    [Column("llm_id")]
    public int? LLMId { get; set; }

    /// <summary>
    /// get/set - The LLM used to evaluate step/action prompts.
    /// </summary>
    public LLM? LLM { get; set; }

    /// <summary>
    /// get - The ordered collection of steps within this profile (ordered by SortOrder).
    /// </summary>
    public virtual List<AutomationStep> Steps { get; } = new List<AutomationStep>();

    /// <summary>
    /// get - The collection of runs recorded for this profile.
    /// </summary>
    public virtual List<AutomationRun> Runs { get; } = new List<AutomationRun>();

    /// <summary>
    /// get - The collection of event schedules that trigger this profile (fired by the scheduler service).
    /// </summary>
    public virtual List<EventSchedule> Events { get; } = new List<EventSchedule>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationProfile object.
    /// </summary>
    protected AutomationProfile() : base() { }

    /// <summary>
    /// Creates a new instance of an AutomationProfile object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    public AutomationProfile(string name) : base(name) { }

    /// <summary>
    /// Creates a new instance of an AutomationProfile object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    public AutomationProfile(int id, string name) : base(id, name) { }
    #endregion
}
