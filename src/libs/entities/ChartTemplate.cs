using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// ChartTemplate class, provides a DB model to manage different types of chart templates.
///     A chart template converts an array of content into a JSON object that is used to generate a dynamic chart.
/// </summary>
[Cache("chart_template")]
[Table("chart_template")]
public class ChartTemplate : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The Razor template to generate the chart.
    /// </summary>
    [Column("template")]
    public string Template { get; set; } = "";

    /// <summary>
    /// get/set - Whether this chart template is publicly available to all users.
    /// </summary>
    [Column("is_public")]
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// get/set - The chart template settings.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - Collection of report templates that use this chart template.
    /// </summary>
    public virtual List<ReportTemplate> ReportTemplates { get; } = new List<ReportTemplate>();

    /// <summary>
    /// get - Collection of report templates that use this chart template (many-to-many).
    /// </summary>
    public virtual List<ReportTemplateChartTemplate> ReportTemplatesManyToMany { get; } = new List<ReportTemplateChartTemplate>();

    /// <summary>
    /// get - Collection of report sections that use this chart template.
    /// </summary>
    public virtual List<ReportSection> ReportSections { get; } = new List<ReportSection>();

    /// <summary>
    /// get - Collection of report sections that use this chart template (many-to-many).
    /// </summary>
    public virtual List<ReportSectionChartTemplate> ReportSectionsManyToMany { get; } = new List<ReportSectionChartTemplate>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChartTemplate object.
    /// </summary>
    protected ChartTemplate() : base() { }

    /// <summary>
    /// Creates a new instance of a ChartTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="template"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ChartTemplate(string name, string template) : base(name)
    {
        this.Template = template ?? throw new ArgumentNullException(nameof(template));
    }

    /// <summary>
    /// Creates a new instance of a ChartTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="template"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ChartTemplate(int id, string name, string template) : base(id, name)
    {
        this.Template = template ?? throw new ArgumentNullException(nameof(template));
    }
    #endregion
}
