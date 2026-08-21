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
/// ComparisonDifference class, the changes only one variant intended for a content item.
/// </summary>
public class ComparisonDifference
{
    public string ContentRef { get; set; } = "";
    public List<string> OnlyA { get; set; } = new();
    public List<string> OnlyB { get; set; } = new();
}
