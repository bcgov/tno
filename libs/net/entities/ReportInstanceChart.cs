using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// ReportInstanceChart class, provides a DB model that holds the chart data computed for a report
/// instance. A report instance is a snapshot, and a chart over an analytics-only section has no
/// content rows to replay, so the computed data is what a later render reads.
/// </summary>
[Table("report_instance_chart")]
public class ReportInstanceChart : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the report instance.
    /// </summary>
    [Key]
    [Column("instance_id")]
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - The report instance.
    /// </summary>
    public ReportInstance? Instance { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the report section.
    /// </summary>
    [Key]
    [Column("section_id")]
    public int SectionId { get; set; }

    /// <summary>
    /// get/set - The report section.
    /// </summary>
    public ReportSection? Section { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the chart template.
    /// </summary>
    [Key]
    [Column("chart_template_id")]
    public int ChartTemplateId { get; set; }

    /// <summary>
    /// get/set - The chart template.
    /// </summary>
    public ChartTemplate? ChartTemplate { get; set; }

    /// <summary>
    /// get/set - The name of the section, as report_instance_content holds it.
    /// </summary>
    [Column("section_name")]
    public string SectionName { get; set; } = "";

    /// <summary>
    /// get/set - The Chart.js data.
    /// </summary>
    [Column("data", TypeName = "jsonb")]
    public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The number of stories counted.
    /// </summary>
    [Column("doc_count")]
    public long DocCount { get; set; }

    /// <summary>
    /// get/set - Whether a bucket list was longer than the aggregation returns.
    /// </summary>
    [Column("is_truncated")]
    public bool IsTruncated { get; set; }

    /// <summary>
    /// get/set - When the data was computed.
    /// </summary>
    [Column("generated_on")]
    public DateTime GeneratedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportInstanceChart object.
    /// </summary>
    protected ReportInstanceChart() { }

    /// <summary>
    /// Creates a new instance of a ReportInstanceChart object, initializes with specified parameters.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="sectionId"></param>
    /// <param name="chartTemplateId"></param>
    /// <param name="sectionName"></param>
    /// <param name="data"></param>
    public ReportInstanceChart(long instanceId, int sectionId, int chartTemplateId, string sectionName, JsonDocument data)
    {
        this.InstanceId = instanceId;
        this.SectionId = sectionId;
        this.ChartTemplateId = chartTemplateId;
        this.SectionName = sectionName;
        this.Data = data;
    }
    #endregion
}
