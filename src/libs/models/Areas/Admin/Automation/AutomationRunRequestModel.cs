namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationRunRequestModel class, parameters for a manual run request.
/// </summary>
public class AutomationRunRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - Optional note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// get/set - Optional trigger source ('manual', 'scheduled'). Defaults to 'manual' when not provided.
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>
    /// get/set - Whether to execute as a dry run: every decision and change is computed and
    /// logged, but no content is written and no reports or notifications are sent.
    /// </summary>
    public bool IsDryRun { get; set; }

    /// <summary>
    /// get/set - Optional candidate definition (raw definition JSON) for a comparison run.
    /// When provided the run executes both the profile's definition (variant 'A') and this
    /// candidate (variant 'B') as dry runs and records the differences. Forces IsDryRun.
    /// </summary>
    public string? CompareDefinition { get; set; }
    #endregion
}
