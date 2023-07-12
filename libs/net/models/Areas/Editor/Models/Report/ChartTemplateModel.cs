using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Report;

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
    public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
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
        this.Settings = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>();
    }
    #endregion
}
