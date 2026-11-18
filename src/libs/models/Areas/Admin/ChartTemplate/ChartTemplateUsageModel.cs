namespace TNO.API.Areas.Admin.Models.ChartTemplate;

/// <summary>
/// ChartTemplateUsageModel class, provides a model for a report that uses a chart template.
/// </summary>
public class ChartTemplateUsageModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the report.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The name of the report.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The report sections that use the chart template.
    /// </summary>
    public IEnumerable<string> Sections { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set - The username of the report owner.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// get/set - Whether the report is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }
    #endregion
}
