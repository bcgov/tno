using System.ComponentModel;

namespace TNO.Entities.Filters;

/// <summary>
/// ConditionalOperator enum, provides a way to control the logic gate for rules.
/// </summary>
public enum ConditionalOperator
{
    /// <summary>
    /// The field must equal the value.
    /// </summary>
    [Description("=")]
    Equal = 0,
    /// <summary>
    /// The field must not equal the value.
    /// </summary>
    [Description("!=")]
    NoEqual = 1,
    /// <summary>
    /// The field must be greater than the value.
    /// </summary>
    [Description(">")]
    GreaterThan = 2,
    /// <summary>
    /// The field must be greater than or equal to the value.
    /// </summary>
    [Description(">=")]
    GreaterThanOrEqualTo = 3,
    /// <summary>
    /// The field must be less than the value.
    /// </summary>
    [Description("<")]
    LessThan = 4,
    /// <summary>
    /// The field must be less than or equal to the value.
    /// </summary>
    [Description("<=")]
    LessThanOrEqualTo = 5,
    /// <summary>
    /// The field must match at least one value in an array.
    /// </summary>
    [Description("any")]
    Any = 6,
    /// <summary>
    /// The field must match at all values in an array.
    /// </summary>
    [Description("all")]
    All = 7,
    /// <summary>
    /// The field must not match any values in an array.
    /// </summary>
    [Description("none")]
    None = 8,
}
