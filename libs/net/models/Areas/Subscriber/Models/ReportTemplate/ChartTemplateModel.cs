using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Subscriber.Models.ReportTemplate;

/// <summary>
/// ChartTemplateModel class, provides a model that represents an report template.
/// </summary>
public class ChartTemplateModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The Razor template to generate the report.
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// get/set - The settings for this report.
    /// </summary>
    public ChartTemplateSettingsModel Settings { get; set; } = new();
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
    #endregion
}
