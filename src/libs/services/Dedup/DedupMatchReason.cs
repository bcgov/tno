namespace TNO.Services.Dedup;

/// <summary>
/// DedupMatchReason enum, why a story was considered a duplicate.
/// </summary>
public enum DedupMatchReason
{
    /// <summary>
    /// The normalized headlines are identical.
    /// </summary>
    ExactHeadline = 0,

    /// <summary>
    /// The headlines share enough words.
    /// </summary>
    FuzzyHeadline = 1,

    /// <summary>
    /// The story text is nearly identical.
    /// </summary>
    Body = 2,
}
