namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// ReportAISectionTestStatus enum, the state of a test run of an AI section.
/// </summary>
public enum ReportAISectionTestStatus
{
    /// <summary>The reporting service has not finished the run.</summary>
    Pending = 0,
    /// <summary>The run produced output.</summary>
    Completed = 1,
    /// <summary>The run failed.</summary>
    Failed = 2,
}
