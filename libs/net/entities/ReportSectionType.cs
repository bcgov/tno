namespace TNO.Entities;

/// <summary>
/// Provides report section types.
/// </summary>
public enum ReportSectionType
{
    /// <summary>
    /// This section displays content.
    /// </summary>
    Content = 0,

    /// <summary>
    /// This section displays a table of contents for the whole report.
    /// </summary>
    TableOfContents = 1,

    /// <summary>
    /// This section displays a summary of the sections above it.
    /// </summary>
    Summary = 2,
}
