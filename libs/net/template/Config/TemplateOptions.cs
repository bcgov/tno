
namespace TNO.TemplateEngine.Config;

/// <summary>
/// TemplateOptions class, configuration options for reporting service
/// </summary>
public class TemplateOptions
{
    #region Properties
    /// <summary>
    /// get/set - The Subscriber App URL.
    /// </summary>
    public Uri? SubscriberAppUrl { get; set; }

    /// <summary>
    /// get/set - The view content URL.
    /// </summary>
    public Uri? ViewContentUrl { get; set; }

    /// <summary>
    /// get/set - The request transcript URL.
    /// </summary>
    public Uri? RequestTranscriptUrl { get; set; }

    /// <summary>
    /// get/set - The add to report URL.
    /// </summary>
    public Uri? AddToReportUrl { get; set; }

    /// <summary>
    /// get/set - Bounded synthesis configuration for direct-model AI sections.
    /// </summary>
    public TNO.AI.Synthesis.SynthesisOptions Synthesis { get; set; } = new();
    #endregion
}
