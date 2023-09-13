using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// FilterModel class, provides a model that represents an filter.
/// </summary>
public class FilterModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to user who owns this report.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The owner of this report.
    /// </summary>
    public UserModel? Owner { get; set; }

    /// <summary>
    /// get/set - The Elasticsearch query.
    /// </summary>
    public JsonDocument Query { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The filter settings.
    /// </summary>
    public FilterSettingsModel Settings { get; set; } = new();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an FilterModel.
    /// </summary>
    public FilterModel() { }

    /// <summary>
    /// Creates a new instance of an FilterModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public FilterModel(Entities.Filter entity, JsonSerializerOptions options) : base(entity)
    {
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.Query = entity.Query;
        this.Settings = JsonSerializer.Deserialize<FilterSettingsModel>(JsonSerializer.Serialize(entity.Settings, options)) ?? new();
    }

    /// <summary>
    /// Creates a new instance of an FilterModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public FilterModel(TNO.API.Areas.Admin.Models.Report.FilterModel model)
    {
        this.Id = model.Id;
        this.Description = model.Description;
        this.Name = model.Name;
        this.IsEnabled = model.IsEnabled;
        this.OwnerId = model.OwnerId;
        this.Owner = model.Owner != null ? new UserModel(model.Owner) : null;
        this.SortOrder = model.SortOrder;
        this.Query = model.Query;
        this.Settings = model.Settings;
    }

    /// <summary>
    /// Creates a new instance of an FilterModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public FilterModel(TNO.API.Areas.Editor.Models.Report.FilterModel model)
    {
        this.Id = model.Id;
        this.Description = model.Description;
        this.Name = model.Name;
        this.IsEnabled = model.IsEnabled;
        this.OwnerId = model.OwnerId;
        this.Owner = model.Owner != null ? new UserModel(model.Owner) : null;
        this.SortOrder = model.SortOrder;
        this.Query = model.Query;
        this.Settings = model.Settings;
    }

    /// <summary>
    /// Creates a new instance of an FilterModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public FilterModel(TNO.API.Areas.Services.Models.Report.FilterModel model)
    {
        this.Id = model.Id;
        this.Description = model.Description;
        this.Name = model.Name;
        this.IsEnabled = model.IsEnabled;
        this.OwnerId = model.OwnerId;
        this.Owner = model.Owner != null ? new UserModel(model.Owner) : null;
        this.SortOrder = model.SortOrder;
        this.Query = model.Query;
        this.Settings = model.Settings;
    }
    #endregion
}
