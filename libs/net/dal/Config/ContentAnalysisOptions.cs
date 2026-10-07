namespace TNO.DAL.Config;

/// <summary>
/// ContentAnalysisOptions class, configuration for Content-Analysis work scheduling.
/// </summary>
public class ContentAnalysisOptions
{
    #region Properties
    /// <summary>
    /// get/set - Seconds after the last input change before content is analyzed. Further changes
    /// inside the quiet period move it forward, so rapid edits produce one analysis of the latest input.
    /// </summary>
    public int QuietPeriodSeconds { get; set; } = 120;

    /// <summary>
    /// get/set - Transient failures retried before a job fails and waits for replay.
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// get/set - Seconds a worker's claim lasts before the job returns to the queue.
    /// </summary>
    public int LeaseSeconds { get; set; } = 600;

    /// <summary>
    /// get/set - Priority of lifecycle and reanalysis work.
    /// </summary>
    public int LifecyclePriority { get; set; } = 100;

    /// <summary>
    /// get/set - Priority of backfill work; lower than lifecycle work.
    /// </summary>
    public int BackfillPriority { get; set; } = 10;
    #endregion
}
