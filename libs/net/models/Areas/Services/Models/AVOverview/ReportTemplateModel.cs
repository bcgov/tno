using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.AVOverview;

/// <summary>
/// ReportTemplateModel class, provides a model that represents an report template.
/// </summary>
public class ReportTemplateModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The report type.
    /// </summary>
    public ReportType ReportType { get; set; }

    /// <summary>
    /// get/set - The Razor subject template to generate the report.
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The Razor body template to generate the report.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - The settings for this report.
    /// </summary>
    public AVOverviewSettingsModel Settings { get; set; } = new();
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
    /// <param name="options"></param>
    public ReportTemplateModel(Entities.ReportTemplate entity, JsonSerializerOptions options) : base(entity)
    {
        this.ReportType = entity.ReportType;
        this.Subject = entity.Subject;
        this.Body = entity.Body;
        this.Settings = JsonSerializer.Deserialize<AVOverviewSettingsModel>(entity.Settings, options) ?? new();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Report object.
    /// </summary>
    /// <returns></returns>
    public Entities.ReportTemplate ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.ReportTemplate)this;
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ReportTemplate(ReportTemplateModel model)
    {
        var entity = new Entities.ReportTemplate(model.Id, model.Name, model.ReportType, model.Subject, model.Body)
        {
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
