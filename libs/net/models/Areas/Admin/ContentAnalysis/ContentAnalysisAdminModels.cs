namespace TNO.API.Areas.Admin.Models.ContentAnalysis;

/// <summary>
/// AnalysisQueueModel class, the analysis job counts.
/// </summary>
public class AnalysisQueueModel
{
    /// <summary>get/set - Counts keyed 'Status:Reason', plus 'Due'.</summary>
    public IDictionary<string, int> Counts { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// AnalysisBackfillRequestModel class, the criteria of a backfill. The start is inclusive and the
/// end exclusive, both UTC; the time zone is the one the administrator chose them in.
/// </summary>
public class AnalysisBackfillRequestModel
{
    /// <summary>get/set - The range start (inclusive, UTC).</summary>
    public DateTime StartOn { get; set; }

    /// <summary>get/set - The range end (exclusive, UTC).</summary>
    public DateTime EndOn { get; set; }

    /// <summary>get/set - The time zone the range was chosen in, shown back to the administrator.</summary>
    public string TimeZone { get; set; } = "";

    /// <summary>get/set - Which content date the range applies to (PublishedOn by default).</summary>
    public TNO.Entities.AnalysisBackfillDateField DateField { get; set; }

    /// <summary>get/set - Missing or stale analysis only (default), or everything.</summary>
    public TNO.Entities.AnalysisBackfillMode Mode { get; set; }
}

/// <summary>
/// AnalysisBackfillPreviewModel class, what a backfill would cover.
/// </summary>
public class AnalysisBackfillPreviewModel
{
    /// <summary>get/set - Eligible content in the range.</summary>
    public int Matching { get; set; }

    /// <summary>get/set - Content excluded by media type or source.</summary>
    public int Excluded { get; set; }

    /// <summary>get/set - Content with no publication date (excluded when the range is by publication date).</summary>
    public int MissingPublicationDate { get; set; }

    /// <summary>get/set - Matching content whose analysis is current (skipped unless forced).</summary>
    public int AlreadyCurrent { get; set; }
}

/// <summary>
/// AnalysisBackfillModel class, a backfill and its progress.
/// </summary>
public class AnalysisBackfillModel : AnalysisBackfillRequestModel
{
    /// <summary>get/set - Primary key.</summary>
    public long Id { get; set; }

    /// <summary>get/set - The backfill status.</summary>
    public TNO.Entities.BackgroundJobStatus Status { get; set; }

    /// <summary>get/set - Content matching the criteria.</summary>
    public int Total { get; set; }

    /// <summary>get/set - Jobs the backfill queued.</summary>
    public int Scheduled { get; set; }

    /// <summary>get/set - Content whose analysis was already current.</summary>
    public int AlreadyCurrent { get; set; }

    /// <summary>get/set - Content analyzed.</summary>
    public int Analyzed { get; set; }

    /// <summary>get/set - Content taken over by lifecycle work.</summary>
    public int Superseded { get; set; }

    /// <summary>get/set - Content deleted after it was scheduled.</summary>
    public int Deleted { get; set; }

    /// <summary>get/set - Jobs that failed (replay them from the job list).</summary>
    public int Failed { get; set; }

    /// <summary>get/set - Jobs still queued or running.</summary>
    public int Remaining { get; set; }

    /// <summary>get/set - Analyzed content that is searchable.</summary>
    public int Indexed { get; set; }

    /// <summary>get/set - Every target is resolved and successful results are searchable.</summary>
    public bool IsComplete { get; set; }

    /// <summary>get/set - The last error.</summary>
    public string? Error { get; set; }

    /// <summary>get/set - Who requested it.</summary>
    public string CreatedBy { get; set; } = "";

    /// <summary>get/set - When it was requested.</summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>get/set - When scheduling finished.</summary>
    public DateTime? CompletedOn { get; set; }
}
