using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// AnalysisRequestModel class, a request to analyze a content item, keyed by content ID so every
/// request for the same content is handled in order. The Content-Analysis service analyzes the
/// content's input as it is when the request is handled; a request whose input has since changed is
/// skipped, since a newer request follows it.
/// </summary>
public class AnalysisRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - Identifies the request; its retries carry the same value.
    /// </summary>
    public string RequestId { get; set; } = "";

    /// <summary>
    /// get/set - The content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Fingerprint of the content's analysis inputs when the request was made.
    /// </summary>
    public string InputHash { get; set; } = "";

    /// <summary>
    /// get/set - Why the content is analyzed.
    /// </summary>
    public AnalysisRequestReason Reason { get; set; }

    /// <summary>
    /// get/set - Analyze even when the content's current analysis is for the same input.
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// get/set - When the request was made.
    /// </summary>
    public DateTime RequestedOn { get; set; }

    /// <summary>
    /// get/set - The backfill work order that sent the request.
    /// </summary>
    public long? WorkOrderId { get; set; }

    /// <summary>
    /// get/set - Failed attempts so far (retry and dead-letter messages).
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// get/set - A retry is not handled before this time.
    /// </summary>
    public DateTime? NotBefore { get; set; }

    /// <summary>
    /// get/set - The last failure (retry and dead-letter messages).
    /// </summary>
    public string? Error { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisRequestModel object.
    /// </summary>
    public AnalysisRequestModel() { }

    /// <summary>
    /// Creates a new instance of an AnalysisRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="contentId"></param>
    /// <param name="inputHash"></param>
    /// <param name="reason"></param>
    /// <param name="force"></param>
    /// <param name="requestedOn"></param>
    public AnalysisRequestModel(string requestId, long contentId, string inputHash, AnalysisRequestReason reason, bool force, DateTime requestedOn)
    {
        this.RequestId = requestId;
        this.ContentId = contentId;
        this.InputHash = inputHash;
        this.Reason = reason;
        this.Force = force;
        this.RequestedOn = requestedOn;
    }
    #endregion
}
