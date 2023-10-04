using RazorEngineCore;
using TNO.API.Models.Settings;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportEngineAVOverviewModel class, provides a model to pass to the razor engine for reports.
/// </summary>
public class ReportEngineAVOverviewModel : RazorEngineTemplateBase
{
    #region Properties
    /// <summary>
    /// get/set - The av overview instance.
    /// </summary>
    public AVOverviewInstanceModel Instance { get; set; } = new();

    /// <summary>
    /// get/set - The report settings.
    /// </summary>
    public AVOverviewSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - The Subscriber app URL.
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportEngineAVOverviewModel.
    /// </summary>
    public ReportEngineAVOverviewModel()
    {
    }

    /// <summary>
    /// Creates a new instance of a ReportEngineAVOverviewModel, initializes with specified parameters.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="settings"></param>
    public ReportEngineAVOverviewModel(AVOverviewInstanceModel instance, AVOverviewSettingsModel settings)
    {
        this.Instance = instance;
        this.Settings = settings;
    }
    #endregion
}
