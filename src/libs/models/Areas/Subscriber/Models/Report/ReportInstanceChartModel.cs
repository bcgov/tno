using System.Text.Json;

namespace TNO.API.Areas.Subscriber.Models.Report;

/// <summary>
/// ReportInstanceChartModel class, provides a model that represents the chart data computed for a report instance.
/// </summary>
public class ReportInstanceChartModel
{
    #region Properties
    /// <summary>
    /// get/set - The report instance.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - The name of the section.
    /// </summary>
    public string SectionName { get; set; } = "";

    /// <summary>
    /// get/set - The section.
    /// </summary>
    public int SectionId { get; set; }

    /// <summary>
    /// get/set - The chart template.
    /// </summary>
    public int ChartTemplateId { get; set; }

    /// <summary>
    /// get/set - The Chart.js data.
    /// </summary>
    public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The number of stories counted.
    /// </summary>
    public long DocCount { get; set; }

    /// <summary>
    /// get/set - Whether a bucket list was longer than the aggregation returns.
    /// </summary>
    public bool IsTruncated { get; set; }

    /// <summary>
    /// get/set - When the data was computed.
    /// </summary>
    public DateTime GeneratedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportInstanceChartModel.
    /// </summary>
    public ReportInstanceChartModel() { }

    /// <summary>
    /// Creates a new instance of an ReportInstanceChartModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ReportInstanceChartModel(Entities.ReportInstanceChart entity)
    {
        this.InstanceId = entity.InstanceId;
        this.SectionName = entity.SectionName;
        this.SectionId = entity.SectionId;
        this.ChartTemplateId = entity.ChartTemplateId;
        this.Data = entity.Data;
        this.DocCount = entity.DocCount;
        this.IsTruncated = entity.IsTruncated;
        this.GeneratedOn = entity.GeneratedOn;
    }
    #endregion
}
