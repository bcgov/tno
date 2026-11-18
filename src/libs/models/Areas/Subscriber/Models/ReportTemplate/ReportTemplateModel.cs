using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;
using TNO.Entities;

namespace TNO.API.Areas.Subscriber.Models.ReportTemplate;

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
    /// get/set - Whether this report template is public to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// get/set - The settings for this report.
    /// </summary>
    public ReportTemplateSettingsModel Settings { get; set; } = new();

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
    /// <param name="options"></param>
    public ReportTemplateModel(Entities.ReportTemplate entity, JsonSerializerOptions options) : base(entity)
    {
        this.ReportType = entity.ReportType;
        this.Subject = entity.Subject;
        this.Body = entity.Body;
        this.IsPublic = entity.IsPublic;
        this.Settings = JsonSerializer.Deserialize<ReportTemplateSettingsModel>(entity.Settings, options) ?? new();

        if (entity.ChartTemplates.Any())
            this.ChartTemplates = entity.ChartTemplates.Select(ct => new ChartTemplateModel(ct, options));
        else if (entity.ChartTemplatesManyToMany.Any())
            this.ChartTemplates = entity.ChartTemplatesManyToMany.Where(ct => ct.ChartTemplate != null).Select(ct => new ChartTemplateModel(ct.ChartTemplate!, options));
    }
    #endregion
}
