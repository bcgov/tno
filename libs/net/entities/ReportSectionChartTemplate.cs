using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// ReportSectionChartTemplate class, provides an entity model that links (many-to-many) report section with chart templates.
/// </summary>
[Table("report_section_chart_template")]
public class ReportSectionChartTemplate : AuditColumns, IEquatable<ReportSectionChartTemplate>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to report section.
    /// </summary>
    [Column("report_section_id")]
    public int ReportSectionId { get; set; }

    /// <summary>
    /// get/set - The report section.
    /// </summary>
    public virtual ReportSection? ReportSection { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to tone pool.
    /// </summary>
    [Column("chart_template_id")]
    public int ChartTemplateId { get; set; }

    /// <summary>
    /// get/set - The chart template.
    /// </summary>
    public virtual ChartTemplate? ChartTemplate { get; set; }

    /// <summary>
    /// get/set - The sort order of the charts within the section.
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - The report section chart settings.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportSectionChartTemplate object.
    /// </summary>
    protected ReportSectionChartTemplate()
    { }

    /// <summary>
    /// Creates a new instance of a ReportSectionChartTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportSection"></param>
    /// <param name="chartTemplate"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ReportSectionChartTemplate(ReportSection reportSection, ChartTemplate chartTemplate)
    {
        this.ReportSectionId = reportSection?.Id ?? throw new ArgumentNullException(nameof(reportSection));
        this.ReportSection = reportSection;
        this.ChartTemplateId = chartTemplate?.Id ?? throw new ArgumentNullException(nameof(chartTemplate));
        this.ChartTemplate = chartTemplate;
    }

    /// <summary>
    /// Creates a new instance of a ReportSectionChartTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportSectionId"></param>
    /// <param name="chartTemplateId"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ReportSectionChartTemplate(int reportSectionId, int chartTemplateId)
    {
        this.ReportSectionId = reportSectionId;
        this.ChartTemplateId = chartTemplateId;
    }

    /// <summary>
    /// Creates a new instance of a ReportSectionChartTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportSectionId"></param>
    /// <param name="chartTemplateId"></param>
    /// <param name="sortOrder"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ReportSectionChartTemplate(int reportSectionId, int chartTemplateId, int sortOrder) : this(reportSectionId, chartTemplateId)
    {
        this.SortOrder = sortOrder;
    }
    #endregion

    #region Methods
    public bool Equals(ReportSectionChartTemplate? other)
    {
        if (other == null) return false;
        return this.ReportSectionId == other.ReportSectionId && this.ChartTemplateId == other.ChartTemplateId;
    }

    public override bool Equals(object? obj) => Equals(obj as ReportSectionChartTemplate);
    public override int GetHashCode() => (this.ReportSectionId, this.ChartTemplateId).GetHashCode();
    #endregion
}
