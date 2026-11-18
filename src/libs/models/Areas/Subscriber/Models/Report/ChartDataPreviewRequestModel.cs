using TNO.API.Models.Settings;

namespace TNO.API.Areas.Subscriber.Models.Report;

/// <summary>
/// ChartDataPreviewRequestModel class, provides the unsaved values of a chart being edited.
/// </summary>
public class ChartDataPreviewRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - The chart settings the form currently holds.
    /// </summary>
    public ChartSectionSettingsModel SectionSettings { get; set; } = new();

    /// <summary>
    /// get/set - Whether the chart counts the stories listed in this report.
    /// </summary>
    public bool UseAllContent { get; set; }

    /// <summary>
    /// get/set - The section's own filter.
    /// </summary>
    public int? FilterId { get; set; }

    /// <summary>
    /// get/set - The section's own folder.
    /// </summary>
    public int? FolderId { get; set; }

    /// <summary>
    /// get/set - The linked report.
    /// </summary>
    public int? LinkedReportId { get; set; }

    /// <summary>
    /// get/set - The unsent instance to take stories from. Defaults to the report's current instance for this user.
    /// </summary>
    public long? InstanceId { get; set; }
    #endregion
}
