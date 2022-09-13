using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.DataSource;

/// <summary>
/// DataSourceModel class, provides a model that represents an category.
/// </summary>
public class DataSourceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public int Id { get; set; }

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
    /// get/set - The data location.
    /// </summary>
    public DataLocationModel? DataLocation { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int ContentTypeId { get; set; }

    /// <summary>
    /// get/set - The content type.
    /// </summary>
    public ContentTypeModel? ContentType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - The media type.
    /// </summary>
    public MediaTypeModel? MediaType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set - The license.
    /// </summary>
    public LicenseModel? License { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The default user who owns the content for this data source..
    /// </summary>
    public UserModel? Owner { get; set; }

    /// <summary>
    /// get/set - The schedule type.
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
    public IEnumerable<SourceActionModel> Actions { get; set; } = Array.Empty<SourceActionModel>();

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<SourceMetricModel> Metrics { get; set; } = Array.Empty<SourceMetricModel>();

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<SourceScheduleModel> Schedules { get; set; } = Array.Empty<SourceScheduleModel>();
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
        this.DataLocation = entity.DataLocation != null ? new DataLocationModel(entity.DataLocation, options) : null;
        this.ContentTypeId = entity.ContentTypeId;
        this.ContentType = entity.ContentType != null ? new ContentTypeModel(entity.ContentType) : null;
        this.MediaTypeId = entity.MediaTypeId;
        this.MediaType = entity.MediaType != null ? new MediaTypeModel(entity.MediaType) : null;
        this.LicenseId = entity.LicenseId;
        this.License = entity.License != null ? new LicenseModel(entity.License) : null;
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.ScheduleType = entity.ScheduleType;
        this.Topic = entity.Topic;
        this.ParentId = entity.ParentId;
        this.Connection = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Connection, options) ?? new Dictionary<string, object>();
        this.RetryLimit = entity.RetryLimit;
        this.LastRanOn = entity.DataService?.LastRanOn;
        this.FailedAttempts = entity.DataService?.FailedAttempts ?? 0;

        this.Actions = entity.ActionsManyToMany.Select(a => new SourceActionModel(a));
        this.Metrics = entity.MetricsManyToMany.Select(m => new SourceMetricModel(m));
        this.Schedules = entity.SchedulesManyToMany.Select(s => new SourceScheduleModel(s));
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
        var entity = new Entities.DataSource(model.Name, model.Code, model.DataLocationId, model.MediaTypeId, model.LicenseId, model.ContentTypeId, model.ScheduleType, model.Topic)
        {
            Id = model.Id,
            ShortName = model.ShortName,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            Connection = JsonSerializer.Serialize(model.Connection),
            OwnerId = model.OwnerId,
            ParentId = model.ParentId,
            RetryLimit = model.RetryLimit,
            DataService = new Entities.DataService(model.Id)
            {
                LastRanOn = model.LastRanOn,
                FailedAttempts = model.FailedAttempts,
            },
            Version = model.Version ?? 0
        };

        entity.ActionsManyToMany.AddRange(model.Actions.Select(a => a.ToEntity(entity.Id)));
        entity.MetricsManyToMany.AddRange(model.Metrics.Select(m => m.ToEntity(entity.Id)));
        entity.SchedulesManyToMany.AddRange(model.Schedules.Select(s => s.ToEntity(entity.Id)));

        return entity;
    }
    #endregion
}
