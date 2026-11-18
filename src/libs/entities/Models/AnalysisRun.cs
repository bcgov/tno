namespace TNO.Entities.Models;

/// <summary>
/// AnalysisRun class, the outcome of one analysis request for a content item, kept in the content's
/// metadata ('analysis.runs'). Retries of a request update its run rather than adding another.
/// </summary>
public class AnalysisRun
{
    #region Properties
    /// <summary>
    /// get/set - Identifies the request; retries carry the same value.
    /// </summary>
    public string RequestId { get; set; } = "";

    /// <summary>
    /// get/set - Why the content was sent for analysis.
    /// </summary>
    public AnalysisRequestReason Reason { get; set; }

    /// <summary>
    /// get/set - The outcome.
    /// </summary>
    public AnalysisRunStatus Status { get; set; }

    /// <summary>
    /// get/set - Fingerprint of the analysis inputs the request was for.
    /// </summary>
    public string InputHash { get; set; } = "";

    /// <summary>
    /// get/set - When the request was made; orders the runs.
    /// </summary>
    public DateTime RequestedOn { get; set; }

    /// <summary>
    /// get/set - When the outcome was recorded.
    /// </summary>
    public DateTime FinishedOn { get; set; }

    /// <summary>
    /// get/set - Attempts made for the request.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// get/set - The backfill work order that sent the request.
    /// </summary>
    public long? WorkOrderId { get; set; }

    /// <summary>
    /// get/set - The analysis the run produced.
    /// </summary>
    public long? AnalysisId { get; set; }

    /// <summary>
    /// get/set - Why the run was skipped, or the last failure.
    /// </summary>
    public string? Error { get; set; }
    #endregion
}

/// <summary>
/// AnalysisMetadata class, a content item's recent analysis runs, newest request first, kept under
/// the 'analysis' key of the content's metadata.
/// </summary>
public class AnalysisMetadata
{
    #region Variables
    /// <summary>
    /// The key in the content's metadata.
    /// </summary>
    public const string Key = "analysis";

    /// <summary>
    /// The number of runs kept.
    /// </summary>
    public const int MaxRuns = 5;
    #endregion

    #region Properties
    /// <summary>
    /// get/set - The outcome of the newest request (runs[0]); indexed to find failed content.
    /// </summary>
    public AnalysisRunStatus? Status { get; set; }

    /// <summary>
    /// get/set - The most recent runs, newest request first.
    /// </summary>
    public List<AnalysisRun> Runs { get; set; } = new();
    #endregion

    #region Methods
    /// <summary>
    /// Record a run. A run for a request already recorded replaces it; the runs are ordered by when
    /// their request was made, so an older request that finishes late never displaces a newer
    /// outcome as the content's status. Only the newest 'MaxRuns' are kept.
    /// </summary>
    /// <param name="run"></param>
    /// <returns>This metadata.</returns>
    public AnalysisMetadata Record(AnalysisRun run)
    {
        this.Runs.RemoveAll(r => r.RequestId == run.RequestId);
        this.Runs.Add(run);
        this.Runs = this.Runs
            .OrderByDescending(r => r.RequestedOn)
            .ThenByDescending(r => r.FinishedOn)
            .Take(MaxRuns)
            .ToList();
        this.Status = this.Runs.FirstOrDefault()?.Status;
        return this;
    }
    #endregion
}
