namespace TNO.Entities;

/// <summary>
/// ReportAIResultStatus enum, the state of a stored AI section result.
/// </summary>
public enum ReportAIResultStatus
{
    /// <summary>
    /// A generator has claimed the result and is producing it.
    /// </summary>
    Pending = 0,
    /// <summary>
    /// The result is complete and reusable.
    /// </summary>
    Completed = 1,
    /// <summary>
    /// Generation failed; the next request tries again.
    /// </summary>
    Failed = 2,
}
