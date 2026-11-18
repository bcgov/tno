namespace TNO.Services.Automation.Engine;

/// <summary>
/// RunSummary class, the JSON outcome persisted on the run. A normal run carries one variant;
/// a comparison run carries both variants plus the per-item differences between their intended
/// change sets.
/// </summary>
public class RunSummary
{
    /// <summary>get/set - The engine version that produced the summary.</summary>
    public int EngineVersion { get; set; } = 2;

    /// <summary>get/set - Whether the run was a dry run (changes computed and logged, none written).</summary>
    public bool IsDryRun { get; set; }

    /// <summary>get/set - Whether the run executed in comparison mode.</summary>
    public bool IsComparison { get; set; }

    /// <summary>get/set - The primary (or only) variant's outcome.</summary>
    public VariantSummary? VariantA { get; set; }

    /// <summary>get/set - The candidate variant's outcome (comparison runs only).</summary>
    public VariantSummary? VariantB { get; set; }

    /// <summary>get/set - Per-item differences between the two variants' intended changes.</summary>
    public List<ComparisonDifference> Differences { get; set; } = new();
}

/// <summary>
/// VariantSummary class, the outcome of executing one definition: per-step counts, the run's
/// cost instrumentation (calls, tokens), every change (intended, on a dry run), final collection
/// sizes, exclusions, and the draft-to-id map recorded at flush.
/// </summary>
public class VariantSummary
{
    public List<StepSummary> Steps { get; set; } = new();
    public List<ChangeSummary> Changes { get; set; } = new();
    public Dictionary<string, int> Collections { get; set; } = new();
    public List<ExclusionSummary> Excluded { get; set; } = new();
    /// <summary>get/set - Draft temp key -> database id, so the review reads sensibly after flush.</summary>
    public Dictionary<string, long> DraftIds { get; set; } = new();
    /// <summary>get/set - Every Score Content action's result, grouped by objective, with the
    /// distribution of the scores it recorded.</summary>
    public List<ScoreObjectiveSummary> Scores { get; set; } = new();
    /// <summary>get/set - Every Select Top Scored action's result: the ranking rule it applied,
    /// the items it kept, and the distribution it chose from.</summary>
    public List<SelectionSummary> Selections { get; set; } = new();
    /// <summary>get/set - Every item a Save Collection / Save Content Now action wrote (or would
    /// write, on a dry run), naming the fields the save carried.</summary>
    public List<SaveSummary> Saves { get; set; } = new();
    public int LlmCalls { get; set; }
    public long PromptTokens { get; set; }
    public long CompletionTokens { get; set; }
    public long DurationMs { get; set; }
    /// <summary>get/set - Items that could not be flushed (with the error), so unwritten changes are visible.</summary>
    public List<string> FlushFailures { get; set; } = new();
}

/// <summary>
/// StepSummary class, per-step counts and cost.
/// </summary>
public class StepSummary
{
    public string Name { get; set; } = "";
    public string Phase { get; set; } = "";
    public int Items { get; set; }
    public int Executions { get; set; }
    public int Skipped { get; set; }
    public int Excluded { get; set; }
    public int Aborted { get; set; }
    public int Failures { get; set; }
    public int LlmCalls { get; set; }
    public long DurationMs { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// ChangeSummary class, one change an action produced (or intended, on a dry run).
/// </summary>
public class ChangeSummary
{
    public string Type { get; set; } = "";
    /// <summary>get/set - The content id, or a draft temp key before flush.</summary>
    public string ContentRef { get; set; } = "";
    public string? Field { get; set; }
    public string? Value { get; set; }
    public string? Step { get; set; }
}

/// <summary>
/// ExclusionSummary class, one excluded item with its reason - exclusions are auditable,
/// never silent.
/// </summary>
public class ExclusionSummary
{
    public string ContentRef { get; set; } = "";
    public string Reason { get; set; } = "";
    public string? Step { get; set; }
}

/// <summary>
/// ScoredItemSummary class, one item and the score recorded for it. The headline travels with
/// the score so the outcome names the story instead of an opaque content id.
/// </summary>
public class ScoredItemSummary
{
    /// <summary>get/set - The content id, or a draft temp key before flush.</summary>
    public string ContentRef { get; set; } = "";

    /// <summary>get/set - The recorded score.</summary>
    public int Score { get; set; }

    /// <summary>get/set - The item's headline at the time it was scored (truncated).</summary>
    public string? Headline { get; set; }

    /// <summary>get/set - The step that recorded the score.</summary>
    public string? Step { get; set; }
}

/// <summary>
/// ScoreObjectiveSummary class, every score recorded under one objective plus the distribution of
/// those scores, so the outcome shows which items a Score Content action scored and at what -
/// not merely how many ran.
/// </summary>
public class ScoreObjectiveSummary
{
    /// <summary>get/set - The objective the scores were recorded under.</summary>
    public string Objective { get; set; } = "";

    /// <summary>get/set - The steps that scored for this objective.</summary>
    public List<string> Steps { get; set; } = new();

    /// <summary>get/set - The scored items, highest score first.</summary>
    public List<ScoredItemSummary> Items { get; set; } = new();

    /// <summary>get/set - Score -> how many items carry it, highest score first.</summary>
    public Dictionary<int, int> Distribution { get; set; } = new();

    /// <summary>get/set - Items whose value was not an integer and so were never scored.</summary>
    public int Unscored { get; set; }
}

/// <summary>
/// SelectionSummary class, one Select Top Scored action's outcome. The action never calls an LLM;
/// it ranks the recorded scores, so the ranking rule is recorded alongside the items it kept and
/// the distribution of the whole candidate pool it chose from.
/// </summary>
public class SelectionSummary
{
    /// <summary>get/set - The objective whose scores were ranked.</summary>
    public string Objective { get; set; } = "";

    /// <summary>get/set - The step that ran the selection.</summary>
    public string? Step { get; set; }

    /// <summary>get/set - The action's name.</summary>
    public string? Action { get; set; }

    /// <summary>get/set - The ranking rule applied (no LLM is involved).</summary>
    public string SortedBy { get; set; } = "";

    /// <summary>get/set - What the action kept, in words ('the top 10', 'every item scoring 7 or
    /// higher, capped at 20').</summary>
    public string Rule { get; set; } = "";

    /// <summary>get/set - The configured count cap, or null when only a score threshold applies.</summary>
    public int? Take { get; set; }

    /// <summary>get/set - The score threshold, or null when a fixed count was taken instead.</summary>
    public int? MinScore { get; set; }

    /// <summary>get/set - How many scored items it ranked.</summary>
    public int Candidates { get; set; }

    /// <summary>get/set - How many candidates met the score threshold before the count cap
    /// (equal to Candidates when no threshold is set).</summary>
    public int Qualified { get; set; }

    /// <summary>get/set - The collection the selected items were written to.</summary>
    public string? Into { get; set; }

    /// <summary>get/set - The content action stamped on each selected item.</summary>
    public string? ContentAction { get; set; }

    /// <summary>get/set - The selected items in rank order.</summary>
    public List<ScoredItemSummary> Selected { get; set; } = new();

    /// <summary>get/set - Score -> how many candidates carry it, highest score first.</summary>
    public Dictionary<int, int> Distribution { get; set; } = new();

    /// <summary>get/set - Ranked keys that no longer resolved to an item and were dropped, so a
    /// short selection is never silent.</summary>
    public List<string> Unresolved { get; set; } = new();
}

/// <summary>
/// SaveSummary class, one item a save action wrote, naming the fields the write carried so the
/// outcome shows what was updated rather than counting the items.
/// </summary>
public class SaveSummary
{
    /// <summary>get/set - The content id, or a draft temp key when the save created the item.</summary>
    public string ContentRef { get; set; } = "";

    /// <summary>get/set - The step that saved it.</summary>
    public string? Step { get; set; }

    /// <summary>get/set - The action's name.</summary>
    public string? Action { get; set; }

    /// <summary>get/set - The collection the item was saved from (Save Collection only).</summary>
    public string? Collection { get; set; }

    /// <summary>get/set - The item's headline (truncated).</summary>
    public string? Headline { get; set; }

    /// <summary>get/set - The fields the save wrote (headline, tags, sentiment, contributor, ...).</summary>
    public List<string> Fields { get; set; } = new();

    /// <summary>get/set - 'saved', 'created', 'would-save' (dry run) or 'failed'.</summary>
    public string Outcome { get; set; } = "";

    /// <summary>get/set - Whether the save also sent the search-index message.</summary>
    public bool Indexed { get; set; }

    /// <summary>get/set - The error when the save failed.</summary>
    public string? Error { get; set; }
}

/// <summary>
/// ComparisonDifference class, the changes only one variant intended for a content item.
/// </summary>
public class ComparisonDifference
{
    public string ContentRef { get; set; } = "";
    public List<string> OnlyA { get; set; } = new();
    public List<string> OnlyB { get; set; } = new();
}
