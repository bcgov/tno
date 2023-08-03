using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Services.Models.Report;

/// <summary>
/// ChartTemplateModel class, provides a model that represents an chart template.
/// </summary>
public class ChartTemplateModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The Razor template to generate the chart.
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// get/set - The settings for this chart template.
    /// </summary>
    public ChartTemplateSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - The settings for this section chart.
    /// </summary>
    public ChartSettingsModel? SectionSettings { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ChartTemplateModel.
    /// </summary>
    public ChartTemplateModel() { }

    /// <summary>
    /// Creates a new instance of an ChartTemplateModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ChartTemplateModel(Entities.ChartTemplate entity, JsonSerializerOptions options) : base(entity)
    {
        this.Template = entity.Template;
        this.Settings = JsonSerializer.Deserialize<ChartTemplateSettingsModel>(entity.Settings, options) ?? new();
    }

    /// <summary>
    /// Creates a new instance of an ChartTemplateModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ChartTemplateModel(Entities.ReportSectionChartTemplate entity, JsonSerializerOptions options) : base(entity.ChartTemplate)
    {
        this.Template = entity.ChartTemplate?.Template ?? throw new ArgumentNullException(nameof(entity));
        this.Settings = JsonSerializer.Deserialize<ChartTemplateSettingsModel>(entity.ChartTemplate.Settings, options) ?? new();
        this.SectionSettings = JsonSerializer.Deserialize<ChartSettingsModel>(entity.Settings, options) ?? new();
    }
    #endregion
}
