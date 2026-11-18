namespace TNO.Entities;

/// <summary>
/// ReportAITestStatus enum, the state of one test run of an AI report section.
/// </summary>
public enum ReportAITestStatus
{
    /// <summary>
    /// The reporting service has not posted an outcome yet.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The section was generated.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// The run failed; see the error.
    /// </summary>
    Failed = 2,
}
