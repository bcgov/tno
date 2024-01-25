using System.Text.Json;
using TNO.API.Models;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportModel class, provides a model that represents an report.
/// </summary>
public class ReportModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to user who owns this report.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The report settings.
    /// </summary>
    public TNO.API.Models.Settings.ReportSettingsModel Settings { get; set; } = new();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportModel.
    /// </summary>
    public ReportModel() { }

    /// <summary>
    /// Creates a new instance of an ReportModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ReportModel(Entities.Report entity, JsonSerializerOptions options) : base(entity)
    {
        this.OwnerId = entity.OwnerId;
        this.Settings = JsonSerializer.Deserialize<TNO.API.Models.Settings.ReportSettingsModel>(JsonSerializer.Serialize(entity.Settings, options), options) ?? new();
    }

    /// <summary>
    /// Creates a new instance of an ReportModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ReportModel(TNO.API.Areas.Admin.Models.Report.ReportModel model)
    {
        this.Id = model.Id;
        this.Description = model.Description;
        this.Name = model.Name;
        this.IsEnabled = model.IsEnabled;
        this.OwnerId = model.OwnerId;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
    }

    /// <summary>
    /// Creates a new instance of an ReportModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ReportModel(TNO.API.Areas.Editor.Models.Report.ReportModel model)
    {
        this.Id = model.Id;
        this.Description = model.Description;
        this.Name = model.Name;
        this.IsEnabled = model.IsEnabled;
        this.OwnerId = model.OwnerId;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
    }

    /// <summary>
    /// Creates a new instance of an ReportModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ReportModel(TNO.API.Areas.Services.Models.Report.ReportModel model)
    {
        this.Id = model.Id;
        this.Description = model.Description;
        this.Name = model.Name;
        this.IsEnabled = model.IsEnabled;
        this.OwnerId = model.OwnerId;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
    }
    #endregion
}
