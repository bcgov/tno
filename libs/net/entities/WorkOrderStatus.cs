namespace TNO.Entities;

/// <summary>
/// Provides work status for work order requests.
/// </summary>
public enum WorkOrderStatus
{
    /// <summary>
    /// A request for work has been submitted.
    /// </summary>
    Submitted = 0,

    /// <summary>
    /// Work is currently being processed.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Work is completed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Work has been cancelled.
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Work has failed and is not complete.
    /// </summary>
    Failed = 4,
}
