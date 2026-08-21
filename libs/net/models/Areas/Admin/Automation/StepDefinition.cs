namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// StepDefinition class, one step of a profile definition.
/// A step declares its lifecycle phase, where its content comes from (process steps only),
/// zero or more named analyses (LLM prompts with declared result shapes), and its ordered actions.
/// Within a step, every action applies to the item the step is iterating (the subject rule).
/// </summary>
public class StepDefinition
{
    #region Properties
    /// <summary>
    /// get/set - Step name (unique within the profile).
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// get/set - Lifecycle phase: 'init' (runs once, first), 'process' (runs per item of the
    /// step's resolved source), or 'complete' (runs once, last).
    /// </summary>
    public string Phase { get; set; } = AutomationPhases.Process;

    /// <summary>
    /// get/set - Whether the step executes.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - Where a process step's content comes from. Required for 'process',
    /// not allowed for 'init'/'complete' (they run once with no subject).
    /// </summary>
    public SourceDefinition? Source { get; set; }


    /// <summary>
    /// get/set - Optional LLM override for this step's analyses.
    /// </summary>
    public int? LlmId { get; set; }

    /// <summary>
    /// get/set - Named analyses: each is one prompt with a declared result shape. Analyses are
    /// lazy - one executes only when a reachable action consumes its result.
    /// </summary>
    public List<AnalysisDefinition> Analyses { get; set; } = new();

    /// <summary>
    /// get/set - Ordered actions. Property conditions evaluate before any prompt is sent.
    /// </summary>
    public List<ActionDefinition> Actions { get; set; } = new();
    #endregion
}

/// <summary>
/// SourceDefinition class, declares where a process step's content comes from.
/// </summary>
public class SourceDefinition
{
    #region Properties
    /// <summary>
    /// get/set - The source kind: 'filter' (the step runs its own search) or 'collection'
    /// (a named collection from the run context; content enters a run through 'search' actions).
    /// </summary>
    public string From { get; set; } = "collection";

    /// <summary>
    /// get/set - The filter to execute when From is 'filter'.
    /// </summary>
    public int? Filter { get; set; }

    /// <summary>
    /// get/set - The collection name (e.g. '$run.digests') when From is 'collection'.
    /// </summary>
    public string? Collection { get; set; }

    /// <summary>
    /// get/set - Gate filters: only items matching every one of these filter ids are processed.
    /// Each distinct filter resolves once per run to an id set.
    /// </summary>
    public List<int> Include { get; set; } = new();

    /// <summary>
    /// get/set - Gate filters: items matching any of these filter ids are skipped.
    /// </summary>
    public List<int> Exclude { get; set; } = new();

    /// <summary>
    /// get/set - Digest field projection override for hydrated items (defaults to the engine's
    /// standard digest field list).
    /// </summary>
    public List<string>? Fields { get; set; }

    /// <summary>
    /// get/set - Maximum items to process (safety cap; truncation is reported in the summary).
    /// </summary>
    public int? Max { get; set; }
    #endregion
}
