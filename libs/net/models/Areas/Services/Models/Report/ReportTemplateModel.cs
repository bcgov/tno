using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Report;

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
    /// get/set - The settings for this report.
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

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
        this.Subject = entity.Subject;
        this.Body = entity.Body;
        this.Settings = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>();

        if (entity.ChartTemplates.Any())
            this.ChartTemplates = entity.ChartTemplates.Select(ct => new ChartTemplateModel(ct, options));
        else if (entity.ChartTemplatesManyToMany.Any())
            this.ChartTemplates = entity.ChartTemplatesManyToMany.Where(ct => ct.ChartTemplate != null).Select(ct => new ChartTemplateModel(ct.ChartTemplate!, options));
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
        var entity = new Entities.ReportTemplate(model.Id, model.Name, model.Subject, model.Body)
        {
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };
        entity.ChartTemplatesManyToMany.AddRange(model.ChartTemplates.Select(ct => new Entities.ReportTemplateChartTemplate(entity.Id, ct.Id)));

        return entity;
    }
    #endregion
}
