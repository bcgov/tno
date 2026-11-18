using TNO.API.Models.Settings;
using TNO.Entities;

namespace TNO.DAL.Models;

/// <summary>
/// ReportChartDataRequest class, provides what is needed to compute the data of one chart.
/// </summary>
public class ReportChartDataRequest
{
    #region Properties
    /// <summary>
    /// get/set - The report, with its sections and their chart templates.
    /// </summary>
    public required Report Report { get; set; }

    /// <summary>
    /// get/set - The media analytics section.
    /// </summary>
    public required ReportSection Section { get; set; }

    /// <summary>
    /// get/set - The chart template in the section.
    /// </summary>
    public required ReportSectionChartTemplate Chart { get; set; }

    /// <summary>
    /// get/set - Where the stories come from.
    /// </summary>
    public required ReportChartSource Source { get; set; }

    /// <summary>
    /// get/set - The content ids of each source section (InstanceIds, LinkedInstanceIds), in section sort order.
    /// </summary>
    public IDictionary<string, long[]> SectionIds { get; set; } = new Dictionary<string, long[]>();

    /// <summary>
    /// get/set - The source sections' labels, by name. A section missing from this is labelled 'Other'.
    /// </summary>
    public IDictionary<string, string?> SectionLabels { get; set; } = new Dictionary<string, string?>();

    /// <summary>
    /// get/set - The user whose source and media type exclusions apply.
    /// </summary>
    public int? RequestorId { get; set; }

    /// <summary>
    /// get/set - The report instance being worked on.
    /// </summary>
    public long? InstanceId { get; set; }

    /// <summary>
    /// get/set - Unsaved chart settings to use instead of those stored on the section.
    /// </summary>
    public ChartSectionSettingsModel? SettingsOverride { get; set; }
    #endregion
}
