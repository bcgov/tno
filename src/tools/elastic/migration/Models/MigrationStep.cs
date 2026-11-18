namespace TNO.Elastic.Migration;

/// <summary>
/// MigrationStep class, provides a model to define how to perform a migration step.
/// </summary>
public class MigrationStep
{
    #region Properties
    /// <summary>
    /// get/set - The action to perform for this step.
    /// </summary>
    public MigrationAction Action { get; set; } = MigrationAction.NotDefined;

    /// <summary>
    /// get/set - A dictionary of settings for this step and action.
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();

    /// <summary>
    /// get/set - The data for the action.
    /// </summary>
    public object? Data { get; set; }
    #endregion
}
