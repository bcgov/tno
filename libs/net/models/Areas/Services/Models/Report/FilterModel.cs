using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Report;

/// <summary>
/// FilterModel class, provides a model that represents an filter.
/// </summary>
public class FilterModel : BaseTypeWithAuditColumnsModel<int>
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
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");
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
    public FilterModel(Entities.Filter entity) : base(entity)
    {
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.Query = entity.Query;
        this.Settings = entity.Settings;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Filter object.
    /// </summary>
    /// <returns></returns>
    public Entities.Filter ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.Filter)this;
        entity.Query = JsonDocument.Parse(JsonSerializer.Serialize(this.Query, options));
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Filter(FilterModel model)
    {
        var entity = new Entities.Filter(model.Id, model.Name, model.OwnerId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            OwnerId = model.OwnerId,
            SortOrder = model.SortOrder,
            Query = JsonDocument.Parse(JsonSerializer.Serialize(model.Query)),
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
