namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationRunStatus enum, identifies lifecycle state of an automation run.
/// </summary>
public enum AutomationRunStatus
{
    /// <summary>
    /// Run has been queued and is waiting to be executed.
    /// </summary>
    Draft = 0,
    /// <summary>
    /// Run has been picked up by the automation service and is executing.
    /// </summary>
    Running = 1,
    /// <summary>
    /// Run completed successfully.
    /// </summary>
    Completed = 2,
    /// <summary>
    /// Run failed to complete.
    /// </summary>
    Failed = 3,
}
