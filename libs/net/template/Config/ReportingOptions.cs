
namespace TNO.TemplateEngine.Config;

/// <summary>
/// ReportingOptions class, configuration options for reporting service
/// </summary>
public class ReportingOptions
{
    #region Properties
    /// <summary>
    /// get/set - The MMIA URL.
    /// </summary>
    public Uri? MmiaUrl { get; set; }

    /// <summary>
    /// get/set - The view content URL.
    /// </summary>
    public Uri? ViewContentUrl { get; set; }

    /// <summary>
    /// get/set - The request transcript URL.
    /// </summary>
    public Uri? RequestTranscriptUrl { get; set; }
    #endregion
}
