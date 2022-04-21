using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.DataSource;

/// <summary>
/// DataSourceModel class, provides a model that represents an category.
/// </summary>
public class DataSourceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public int Id { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string ShortName { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int DataLocationId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public DataLocationModel? DataLocation { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public MediaTypeModel? MediaType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public LicenseModel? License { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public DataSourceScheduleType ScheduleType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string Topic { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public Dictionary<string, object> Connection { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? LastRanOn { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int RetryLimit { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<SourceScheduleModel> DataSourceSchedules { get; set; } = Array.Empty<SourceScheduleModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an DataSourceModel.
    /// </summary>
    public DataSourceModel() { }

    /// <summary>
    /// Creates a new instance of an DataSourceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public DataSourceModel(Entities.DataSource entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.Code = entity.Code;
        this.Name = entity.Name;
        this.ShortName = entity.ShortName;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.DataLocationId = entity.DataLocationId;
        this.DataLocation = entity.DataLocation != null ? new DataLocationModel(entity.DataLocation) : null;
        this.MediaTypeId = entity.MediaTypeId;
        this.MediaType = entity.MediaType != null ? new MediaTypeModel(entity.MediaType) : null;
        this.LicenseId = entity.LicenseId;
        this.License = entity.License != null ? new LicenseModel(entity.License) : null;
        this.ScheduleType = entity.ScheduleType;
        this.Topic = entity.Topic;
        this.ParentId = entity.ParentId;
        this.Connection = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Connection, options) ?? new Dictionary<string, object>();
        this.LastRanOn = entity.LastRanOn;
        this.RetryLimit = entity.RetryLimit;
        this.FailedAttempts = entity.FailedAttempts;

        this.DataSourceSchedules = entity.SchedulesManyToMany.Select(s => new SourceScheduleModel(s));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a DataSource object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.DataSource ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.DataSource)this;
        entity.Connection = JsonSerializer.Serialize(this.Connection, options);
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.DataSource(DataSourceModel model)
    {
        var entity = new Entities.DataSource(model.Name, model.Code, model.DataLocationId, model.MediaTypeId, model.LicenseId, model.ScheduleType, model.Topic)
        {
            Id = model.Id,
            ShortName = model.ShortName,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            Connection = JsonSerializer.Serialize(model.Connection),
            ParentId = model.ParentId,
            LastRanOn = model.LastRanOn,
            RetryLimit = model.RetryLimit,
            FailedAttempts = model.FailedAttempts,
            Version = model.Version ?? 0
        };

        if (model.DataSourceSchedules.Any())
            entity.Schedules.AddRange(model.DataSourceSchedules.Where(ds => ds.Schedule != null).Select(ds => (Entities.Schedule)ds));

        return entity;
    }
    #endregion
}
