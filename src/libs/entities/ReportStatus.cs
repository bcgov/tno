namespace TNO.Entities;
/// <summary>
/// Provides report status.
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// Report is pending to be sent.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Report has been submitted to be sent to subscribers.
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// Report is accepted by CHES.
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// Report is sent.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Report was cancelled.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Report failed to send.
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Report has been reopened after being sent.
    /// </summary>
    Reopen = 6,
}
