namespace TNO.Kafka.Models;

/// <summary>
/// Label class, provides a model that represents a Label to identify content information.
/// TODO: Change name to LabelModel for consistent naming convention
/// </summary>
public class LabelModel
{
    #region Properties
    /// <summary>
    /// get/set - The Label key or a way to group related information.
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// get/set - The Label value to identify unique information about content.
    /// </summary>
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Label object.
    /// </summary>
    public LabelModel() { }

    /// <summary>
    /// Creates a new instance of a Label object, initializes with specified parameters.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public LabelModel(string key, string value)
    {
        this.Key = key ?? "";
        this.Value = value ?? "";
    }
    #endregion
}
