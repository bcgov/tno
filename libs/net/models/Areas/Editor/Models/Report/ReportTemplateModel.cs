using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Report;

/// <summary>
/// ReportTemplateModel class, provides a model that represents an report template.
/// </summary>
public class ReportTemplateModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The Razor subject template to generate the report.
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The Razor body template to generate the report.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - Whether this report template supports sections.
    /// </summary>
    public bool EnableSections { get; set; } = false;

    /// <summary>
    /// get/set - Whether this report template supports section summaries.
    /// </summary>
    public bool EnableSectionSummary { get; set; } = false;

    /// <summary>
    /// get/set - Whether this report template supports full summary.
    /// </summary>
    public bool EnableSummary { get; set; } = false;

    /// <summary>
    /// get/set - Whether this report template supports charts.
    /// </summary>
    public bool EnableCharts { get; set; } = false;

    /// <summary>
    /// get/set - Whether this report template supports charts over time.
    /// </summary>
    public bool EnableChartsOverTime { get; set; } = false;

    /// <summary>
    /// get/set - An array of chart templates.
    /// </summary>
    public IEnumerable<ChartTemplateModel> ChartTemplates { get; set; } = Array.Empty<ChartTemplateModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportTemplateModel.
    /// </summary>
    public ReportTemplateModel() { }

    /// <summary>
    /// Creates a new instance of an ReportTemplateModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ReportTemplateModel(Entities.ReportTemplate entity) : base(entity)
    {
        this.Subject = entity.Subject;
        this.Body = entity.Body;
        this.EnableSections = entity.EnableSections;
        this.EnableSectionSummary = entity.EnableSectionSummary;
        this.EnableSummary = entity.EnableSummary;
        this.EnableCharts = entity.EnableCharts;
        this.EnableChartsOverTime = entity.EnableChartsOverTime;

        if (entity.ChartTemplates.Any())
            this.ChartTemplates = entity.ChartTemplates.Select(ct => new ChartTemplateModel(ct));
        else if (entity.ChartTemplatesManyToMany.Any())
            this.ChartTemplates = entity.ChartTemplatesManyToMany.Where(ct => ct.ChartTemplate != null).Select(ct => new ChartTemplateModel(ct.ChartTemplate!));
    }
    #endregion
}
