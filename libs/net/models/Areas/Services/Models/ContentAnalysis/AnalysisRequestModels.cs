using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.API.Areas.Services.Models.ContentAnalysis;

/// <summary>
/// AnalysisRequestRunModel class, identifies the analysis request an outcome belongs to, so the
/// outcome is recorded as that request's run in the content's metadata.
/// </summary>
public class AnalysisRequestRunModel
{
    /// <summary>get/set - Identifies the request; its retries carry the same value.</summary>
    public string RequestId { get; set; } = "";

    /// <summary>get/set - Why the content was sent for analysis.</summary>
    public AnalysisRequestReason Reason { get; set; }

    /// <summary>get/set - Fingerprint of the analysis inputs the request was for.</summary>
    public string InputHash { get; set; } = "";

    /// <summary>get/set - When the request was made.</summary>
    public DateTime RequestedOn { get; set; }

    /// <summary>get/set - Attempts made for the request, including this one.</summary>
    public int Attempts { get; set; }

    /// <summary>get/set - The backfill work order that sent the request.</summary>
    public long? WorkOrderId { get; set; }

    /// <summary>
    /// The run this request's outcome records.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="error"></param>
    /// <param name="analysisId"></param>
    /// <returns></returns>
    public AnalysisRun ToRun(AnalysisRunStatus status, string? error = null, long? analysisId = null)
    {
        return new AnalysisRun()
        {
            RequestId = this.RequestId,
            Reason = this.Reason,
            Status = status,
            InputHash = this.InputHash,
            RequestedOn = this.RequestedOn,
            FinishedOn = DateTime.UtcNow,
            Attempts = this.Attempts,
            WorkOrderId = this.WorkOrderId,
            AnalysisId = analysisId,
            Error = error != null && error.Length > 4000 ? error[..4000] : error,
        };
    }
}

/// <summary>
/// AnalysisRunRequestModel class, the outcome of an analysis request that produced no result
/// (skipped, retrying, or failed).
/// </summary>
public class AnalysisRunRequestModel : AnalysisRequestRunModel
{
    /// <summary>get/set - The outcome.</summary>
    public AnalysisRunStatus Status { get; set; }

    /// <summary>get/set - Why it was skipped, or the failure.</summary>
    public string? Error { get; set; }
}

/// <summary>
/// AnalysisInputModel class, a content item's current analysis input.
/// </summary>
public class AnalysisInputModel
{
    /// <summary>get/set - The content.</summary>
    public long ContentId { get; set; }

    /// <summary>get/set - Fingerprint of the content's current analysis inputs.</summary>
    public string InputHash { get; set; } = "";

    /// <summary>get/set - The input fingerprint of the content's current analysis, if any.</summary>
    public string? AnalysisInputHash { get; set; }

    /// <summary>get/set - Whether the content may be analyzed.</summary>
    public bool IsEligible { get; set; }

    /// <summary>get/set - Why it may not be.</summary>
    public string? IneligibleReason { get; set; }

    /// <summary>get/set - The content type.</summary>
    public ContentType ContentType { get; set; }

    /// <summary>get/set - The headline.</summary>
    public string Headline { get; set; } = "";

    /// <summary>get/set - The byline.</summary>
    public string Byline { get; set; } = "";

    /// <summary>get/set - The body or transcript.</summary>
    public string Body { get; set; } = "";

    /// <summary>get/set - A person's summary; empty when analysis owns it.</summary>
    public string Summary { get; set; } = "";

    /// <summary>get/set - The source name.</summary>
    public string Source { get; set; } = "";

    /// <summary>get/set - The source.</summary>
    public int? SourceId { get; set; }

    /// <summary>get/set - The media type name.</summary>
    public string MediaType { get; set; } = "";

    /// <summary>get/set - The media type.</summary>
    public int MediaTypeId { get; set; }

    /// <summary>get/set - The series name.</summary>
    public string Series { get; set; } = "";

    /// <summary>get/set - When it was published.</summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>get/set - Whether the transcript is approved.</summary>
    public bool IsApproved { get; set; }

    /// <summary>get/set - The LLM configured for analysis.</summary>
    public int? LLMId { get; set; }
}

/// <summary>
/// AnalysisBackfillConfigurationModel class, a backfill's criteria and progress, kept in its work
/// order's configuration. The start is inclusive and the end exclusive, both UTC.
/// </summary>
public class AnalysisBackfillConfigurationModel
{
    /// <summary>get/set - The start of the range.</summary>
    public DateTime StartOn { get; set; }

    /// <summary>get/set - The end of the range (exclusive).</summary>
    public DateTime EndOn { get; set; }

    /// <summary>get/set - The time zone the range was chosen in, for display.</summary>
    public string TimeZone { get; set; } = "";

    /// <summary>get/set - Which content date the range applies to.</summary>
    public AnalysisBackfillDateField DateField { get; set; }

    /// <summary>get/set - Which content is analyzed.</summary>
    public AnalysisBackfillMode Mode { get; set; }

    /// <summary>get/set - Content created after the backfill started is left to lifecycle analysis.</summary>
    public DateTime HighWaterMark { get; set; }

    /// <summary>get/set - The last content ID sent; the next page starts after it.</summary>
    public long CheckpointContentId { get; set; }

    /// <summary>get/set - Eligible content in the range when the backfill started.</summary>
    public int Total { get; set; }

    /// <summary>get/set - Content sent for analysis.</summary>
    public int Scheduled { get; set; }

    /// <summary>get/set - Content skipped because its analysis was current.</summary>
    public int AlreadyCurrent { get; set; }

    /// <summary>get/set - Why the backfill stopped.</summary>
    public string? Error { get; set; }
}

/// <summary>
/// AnalysisBackfillItemModel class, a content item a backfill sends for analysis.
/// </summary>
public class AnalysisBackfillItemModel
{
    /// <summary>get/set - The content.</summary>
    public long ContentId { get; set; }

    /// <summary>get/set - Fingerprint of its current analysis inputs.</summary>
    public string InputHash { get; set; } = "";
}

/// <summary>
/// AnalysisBackfillPageModel class, the next page of a backfill.
/// </summary>
public class AnalysisBackfillPageModel
{
    /// <summary>get/set - The content to send for analysis.</summary>
    public IEnumerable<AnalysisBackfillItemModel> Items { get; set; } = Array.Empty<AnalysisBackfillItemModel>();

    /// <summary>get/set - Content in the page skipped because its analysis was current.</summary>
    public int AlreadyCurrent { get; set; }

    /// <summary>get/set - The last content ID in the page; the next page starts after it.</summary>
    public long LastContentId { get; set; }

    /// <summary>get/set - Whether no content is left after this page.</summary>
    public bool IsLast { get; set; }
}
