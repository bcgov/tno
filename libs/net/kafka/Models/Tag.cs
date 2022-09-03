namespace TNO.Kafka.Models;

/// <summary>
/// Tag class, provides a model that represents a tag to identify content information.
/// TODO: Change name to TagModel for consistent naming convention
/// </summary>
public class Tag
{
    #region Properties
    /// <summary>
    /// get/set - The tag key or a way to group related information.
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// get/set - The tag value to identify unique information about content.
    /// </summary>
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Tag object.
    /// </summary>
    public Tag() { }

    /// <summary>
    /// Creates a new instance of a Tag object, initializes with specified parameters.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Tag(string key, string value)
    {
        this.Key = key ?? "";
        this.Value = value ?? "";
    }
    #endregion
}
