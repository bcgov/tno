namespace TNO.Entities;

/// <summary>
/// AnalysisRequestReason enum, why content is sent for analysis.
/// </summary>
public enum AnalysisRequestReason
{
    /// <summary>Content was added, or an analysis input changed.</summary>
    Lifecycle = 0,
    /// <summary>An editor asked for the content to be analyzed again.</summary>
    Reanalysis = 1,
    /// <summary>An administrator's date-range backfill.</summary>
    Backfill = 2,
    /// <summary>An administrator replayed a failed analysis.</summary>
    Replay = 3,
}

/// <summary>
/// AnalysisRunStatus enum, the outcome of an analysis request, recorded in the content's metadata.
/// </summary>
public enum AnalysisRunStatus
{
    /// <summary>A result for the requested input was accepted (or already existed).</summary>
    Completed = 0,
    /// <summary>The content is not eligible (excluded, or awaiting transcript approval).</summary>
    Skipped = 1,
    /// <summary>An attempt failed and the request was sent to the retry topic.</summary>
    Retrying = 2,
    /// <summary>Retries were exhausted, or the failure is permanent; shown for explicit replay.</summary>
    Failed = 3,
}

/// <summary>
/// FieldOwner enum, who set an editorial value.
/// </summary>
public enum FieldOwner
{
    /// <summary>An editor or subscriber, or any value that existed before ownership was recorded.</summary>
    Human = 0,
    /// <summary>Content-Analysis populated it.</summary>
    Analysis = 1,
    /// <summary>The automation engine wrote it.</summary>
    Automation = 2,
}

/// <summary>
/// AnalysisProcess enum, the processes a Content-Analysis service is configured to run. Each process
/// both produces its part of the analysis and, where it has one, applies it to the content's empty field.
/// </summary>
[Flags]
public enum AnalysisProcess
{
    /// <summary>Nothing.</summary>
    None = 0,
    /// <summary>Extract key facts, people and organizations, places, events, and topics (stored and indexed).</summary>
    Metadata = 1,
    /// <summary>Write a summary; fills an empty content summary.</summary>
    Summary = 2,
    /// <summary>Extract verbatim quotes; adds them to the content.</summary>
    Quotes = 4,
    /// <summary>Suggest existing tags; adds them to the content.</summary>
    Tags = 8,
    /// <summary>Suggest the contributor from the byline or columnist; fills an empty contributor.</summary>
    Contributor = 16,
    /// <summary>Choose the primary topic; assigns it to the content per the topic population mode.</summary>
    Topics = 32,
    /// <summary>Every process.</summary>
    All = Metadata | Summary | Quotes | Tags | Contributor | Topics,
}

/// <summary>
/// AnalysisBackfillDateField enum, which content date a backfill range applies to.
/// </summary>
public enum AnalysisBackfillDateField
{
    /// <summary>The publication date.</summary>
    PublishedOn = 0,
    /// <summary>The creation date.</summary>
    CreatedOn = 1,
}

/// <summary>
/// AnalysisBackfillMode enum, which content a backfill analyzes.
/// </summary>
public enum AnalysisBackfillMode
{
    /// <summary>Only content with missing or stale analysis.</summary>
    MissingOrStale = 0,
    /// <summary>Every matching content item, even when its analysis is current.</summary>
    Force = 1,
}
