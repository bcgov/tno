using TNO.Entities;

namespace TNO.API.Areas.Services.Models.ContentAnalysis;

/// <summary>
/// AnalysisClaimRequestModel class, a worker asking for due analysis work.
/// </summary>
public class AnalysisClaimRequestModel
{
    /// <summary>get/set - Identifies the worker (host and process).</summary>
    public string WorkerId { get; set; } = "";

    /// <summary>get/set - The most jobs to claim.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>get/set - The most backfill jobs among them, keeping backfill to its share.</summary>
    public int MaxBackfill { get; set; } = 1;
}

/// <summary>
/// AnalysisJobModel class, claimed analysis work.
/// </summary>
public class AnalysisJobModel
{
    /// <summary>get/set - The job.</summary>
    public long Id { get; set; }

    /// <summary>get/set - The content to analyze.</summary>
    public long ContentId { get; set; }

    /// <summary>get/set - The input fingerprint the job was scheduled for.</summary>
    public string InputHash { get; set; } = "";

    /// <summary>get/set - Why the content is queued.</summary>
    public AnalysisJobReason Reason { get; set; }

    /// <summary>get/set - The job status.</summary>
    public AnalysisJobStatus Status { get; set; }

    /// <summary>get/set - The priority.</summary>
    public int Priority { get; set; }

    /// <summary>get/set - The token a submission must carry.</summary>
    public long FencingToken { get; set; }

    /// <summary>get/set - When the claim lapses.</summary>
    public DateTime? LeaseExpiresOn { get; set; }

    /// <summary>get/set - Attempts made.</summary>
    public int Attempts { get; set; }

    /// <summary>get/set - The backfill that queued the job.</summary>
    public long? BackfillId { get; set; }

    /// <summary>get/set - When the job is due.</summary>
    public DateTime DueOn { get; set; }

    /// <summary>get/set - The last failure.</summary>
    public string? LastError { get; set; }

    /// <summary>get/set - When the job was last updated.</summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// Creates a new instance of an AnalysisJobModel.
    /// </summary>
    public AnalysisJobModel() { }

    /// <summary>
    /// Creates a new instance of an AnalysisJobModel, initializes with specified parameters.
    /// </summary>
    /// <param name="entity"></param>
    public AnalysisJobModel(AnalysisJob entity)
    {
        this.Id = entity.Id;
        this.ContentId = entity.ContentId;
        this.InputHash = entity.InputHash;
        this.Reason = entity.Reason;
        this.Status = entity.Status;
        this.Priority = entity.Priority;
        this.FencingToken = entity.FencingToken;
        this.LeaseExpiresOn = entity.LeaseExpiresOn;
        this.Attempts = entity.Attempts;
        this.BackfillId = entity.BackfillId;
        this.DueOn = entity.DueOn;
        this.LastError = entity.LastError;
        this.UpdatedOn = entity.UpdatedOn;
    }
}

/// <summary>
/// AnalysisLeaseModel class, identifies a claim: the job and its fencing token.
/// </summary>
public class AnalysisLeaseModel
{
    /// <summary>get/set - The job.</summary>
    public long JobId { get; set; }

    /// <summary>get/set - The claim's fencing token.</summary>
    public long FencingToken { get; set; }
}

/// <summary>
/// AnalysisFailureModel class, a worker reporting a failed attempt.
/// </summary>
public class AnalysisFailureModel : AnalysisLeaseModel
{
    /// <summary>get/set - What went wrong.</summary>
    public string Error { get; set; } = "";

    /// <summary>get/set - A transient failure is retried with backoff; a permanent one fails the job.</summary>
    public bool IsTransient { get; set; } = true;
}

/// <summary>
/// AnalysisInputModel class, the current input of claimed content.
/// </summary>
public class AnalysisInputModel
{
    /// <summary>get/set - The job.</summary>
    public long JobId { get; set; }

    /// <summary>get/set - The content.</summary>
    public long ContentId { get; set; }

    /// <summary>get/set - The fingerprint of this input; the submission must carry it.</summary>
    public string InputHash { get; set; } = "";

    /// <summary>get/set - Whether the content is analyzed.</summary>
    public bool IsEligible { get; set; }

    /// <summary>get/set - Why the content is not analyzed.</summary>
    public string? IneligibleReason { get; set; }

    /// <summary>get/set - The content type.</summary>
    public ContentType ContentType { get; set; }

    /// <summary>get/set - The headline.</summary>
    public string Headline { get; set; } = "";

    /// <summary>get/set - The byline.</summary>
    public string Byline { get; set; } = "";

    /// <summary>get/set - The body (HTML or text).</summary>
    public string Body { get; set; } = "";

    /// <summary>get/set - The summary, only when a person wrote it.</summary>
    public string Summary { get; set; } = "";

    /// <summary>get/set - The source name.</summary>
    public string Source { get; set; } = "";

    /// <summary>get/set - The source ID.</summary>
    public int? SourceId { get; set; }

    /// <summary>get/set - The media type name.</summary>
    public string MediaType { get; set; } = "";

    /// <summary>get/set - The media type ID.</summary>
    public int MediaTypeId { get; set; }

    /// <summary>get/set - The series name.</summary>
    public string Series { get; set; } = "";

    /// <summary>get/set - The publication date.</summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>get/set - Whether an audio/video transcript is approved.</summary>
    public bool IsApproved { get; set; }

    /// <summary>get/set - The LLM to use.</summary>
    public int? LLMId { get; set; }
}
