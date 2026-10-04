namespace TNO.TemplateEngine;

/// <summary>
/// AISectionWait enum, how report generation waits for its AI sections.
/// </summary>
public enum AISectionWait
{
    /// <summary>
    /// Generate each missing section, or wait for the generator that holds it. A sent report is
    /// complete.
    /// </summary>
    Wait = 0,

    /// <summary>
    /// Generate each missing section, and leave one another generator holds (background preparation).
    /// </summary>
    Prepare = 1,

    /// <summary>
    /// Use stored results only, and leave a missing section pending (previews).
    /// </summary>
    NoWait = 2,
}
