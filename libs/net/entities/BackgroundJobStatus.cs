namespace TNO.Entities;

/// <summary>
/// BackgroundJobStatus enum, the lifecycle of a long-running administrative job.
/// </summary>
public enum BackgroundJobStatus
{
    /// <summary>
    /// The job has been requested and has not started.
    /// </summary>
    Pending = 0,
    /// <summary>
    /// The job is running.
    /// </summary>
    Running = 1,
    /// <summary>
    /// The job finished; failures, if any, are counted on the job.
    /// </summary>
    Completed = 2,
    /// <summary>
    /// The job stopped because of an error.
    /// </summary>
    Failed = 3,
    /// <summary>
    /// The job was cancelled before it finished.
    /// </summary>
    Cancelled = 4,
}
