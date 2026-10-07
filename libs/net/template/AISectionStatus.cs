namespace TNO.TemplateEngine;

/// <summary>
/// AISectionStatus enum, the state of an AI section after report generation.
/// </summary>
public enum AISectionStatus
{
    /// <summary>
    /// The section has its output.
    /// </summary>
    Ready = 0,

    /// <summary>
    /// The section could not be generated.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// No generator has started the section.
    /// </summary>
    NotStarted = 2,

    /// <summary>
    /// A generator is producing the section.
    /// </summary>
    Generating = 3,
}
