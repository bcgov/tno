namespace TNO.API.Areas.Admin.Models.ContentAnalysis;

/// <summary>
/// AnalysisQueueModel class, the analysis requests waiting in each Kafka topic.
/// </summary>
public class AnalysisQueueModel
{
    /// <summary>get/set - Messages the Content-Analysis consumer group has not handled, keyed by topic.</summary>
    public IDictionary<string, long> Lag { get; set; } = new Dictionary<string, long>();

    /// <summary>get/set - Content whose newest analysis request failed.</summary>
    public int Failed { get; set; }
}

/// <summary>
/// AnalysisFailureModel class, content whose newest analysis request failed.
/// </summary>
public class AnalysisFailureModel
{
    /// <summary>get/set - The content.</summary>
    public long ContentId { get; set; }

    /// <summary>get/set - Its headline.</summary>
    public string Headline { get; set; } = "";

    /// <summary>get/set - The failed run.</summary>
    public TNO.Entities.Models.AnalysisRun Run { get; set; } = new();
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
/// AnalysisBackfillModel class, a backfill (a Content-Analysis backfill work order) and its progress.
/// </summary>
public class AnalysisBackfillModel : AnalysisBackfillRequestModel
{
    /// <summary>get/set - The work order.</summary>
    public long Id { get; set; }

    /// <summary>get/set - The work order status.</summary>
    public TNO.Entities.WorkOrderStatus Status { get; set; }

    /// <summary>get/set - Eligible content in the range when the backfill started.</summary>
    public int Total { get; set; }

    /// <summary>get/set - Content sent for analysis.</summary>
    public int Scheduled { get; set; }

    /// <summary>get/set - Content whose analysis was already current.</summary>
    public int AlreadyCurrent { get; set; }

    /// <summary>get/set - Content whose newest analysis request, from this backfill, failed.</summary>
    public int Failed { get; set; }

    /// <summary>get/set - Why the backfill stopped.</summary>
    public string? Error { get; set; }

    /// <summary>get/set - Who requested it.</summary>
    public string CreatedBy { get; set; } = "";

    /// <summary>get/set - When it was requested.</summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>get/set - When it was last updated.</summary>
    public DateTime? UpdatedOn { get; set; }
}
