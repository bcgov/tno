namespace TNO.Tools.Import.Destination;

/// <summary>
/// Provides content status to determine the stage the content is in.
/// Content status represents either what process should be performed.
/// </summary>
public enum ValueType
{
    /// <summary>
    /// Boolean values.
    /// </summary>
    Boolean = 0,

    /// <summary>
    /// String values.
    /// </summary>
    String = 1,

    /// <summary>
    /// Text values.
    /// </summary>
    Text = 2,

    /// <summary>
    /// Numeric values.
    /// </summary>
    Numeric = 3
}