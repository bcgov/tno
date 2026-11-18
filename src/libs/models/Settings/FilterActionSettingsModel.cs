namespace TNO.API.Models.Settings;

public class FilterActionSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key of action.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The value type of the action.
    /// </summary>
    public string ValueType { get; set; } = "";

    /// <summary>
    /// get/set - The value to filter on.
    /// </summary>
    public string Value { get; set; } = "";
    #endregion
}
