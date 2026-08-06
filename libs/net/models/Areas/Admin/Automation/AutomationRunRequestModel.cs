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
    #endregion
}
