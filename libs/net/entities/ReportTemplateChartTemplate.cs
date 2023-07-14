using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ReportTemplateChartTemplate class, provides an entity model that links (many-to-many) report templates and chart templates.
/// </summary>
[Table("report_template_chart_template")]
public class ReportTemplateChartTemplate : AuditColumns, IEquatable<ReportTemplateChartTemplate>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to report template.
    /// </summary>
    [Column("report_template_id")]
    public int ReportTemplateId { get; set; }

    /// <summary>
    /// get/set - The report template.
    /// </summary>
    public virtual ReportTemplate? ReportTemplate { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to tone pool.
    /// </summary>
    [Column("chart_template_id")]
    public int ChartTemplateId { get; set; }

    /// <summary>
    /// get/set - The chart template.
    /// </summary>
    public virtual ChartTemplate? ChartTemplate { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportTemplateChartTemplate object.
    /// </summary>
    protected ReportTemplateChartTemplate() { }

    /// <summary>
    /// Creates a new instance of a ReportTemplateChartTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportTemplate"></param>
    /// <param name="chartTemplate"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ReportTemplateChartTemplate(ReportTemplate reportTemplate, ChartTemplate chartTemplate)
    {
        this.ReportTemplateId = reportTemplate?.Id ?? throw new ArgumentNullException(nameof(reportTemplate));
        this.ReportTemplate = reportTemplate;
        this.ChartTemplateId = chartTemplate?.Id ?? throw new ArgumentNullException(nameof(chartTemplate));
        this.ChartTemplate = chartTemplate;
    }

    /// <summary>
    /// Creates a new instance of a ReportTemplateChartTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportTemplateId"></param>
    /// <param name="chartTemplateId"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ReportTemplateChartTemplate(int reportTemplateId, int chartTemplateId)
    {
        this.ReportTemplateId = reportTemplateId;
        this.ChartTemplateId = chartTemplateId;
    }
    #endregion

    #region Methods
    public bool Equals(ReportTemplateChartTemplate? other)
    {
        if (other == null) return false;
        return this.ReportTemplateId == other.ReportTemplateId && this.ChartTemplateId == other.ChartTemplateId;
    }

    public override bool Equals(object? obj) => Equals(obj as ReportTemplateChartTemplate);
    public override int GetHashCode() => (this.ReportTemplateId, this.ChartTemplateId).GetHashCode();
    #endregion
}
