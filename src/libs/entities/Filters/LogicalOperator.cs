namespace TNO.Entities.Filters;

/// <summary>
/// LogicalOperator enum, provides a way to control the logic gate for rules.
/// </summary>
public enum LogicalOperator
{
    /// <summary>
    /// Each rule must result in true.
    /// </summary>
    And = 0,
    /// <summary>
    /// One of the rules must result in true.
    /// </summary>
    Or = 1,
    /// <summary>
    /// Only one of the rules must be true.
    /// </summary>
    Xor = 2,
}
