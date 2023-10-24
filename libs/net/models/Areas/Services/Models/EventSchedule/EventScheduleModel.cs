using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.EventSchedule;

/// <summary>
/// EventScheduleModel class, provides a model that represents an event schedule configuration.
/// </summary>
public class EventScheduleModel : AuditColumnsModel
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
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public ScheduleModel? Schedule { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public EventScheduleType EventType { get; set; }

    /// <summary>
    /// get/set - Foreign key to the notification.
    /// </summary>
    public int? NotificationId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report.
    /// </summary>
    public int? ReportId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the folder.
    /// </summary>
    public int? FolderId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? RequestSentOn { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? LastRanOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an EventScheduleModel.
    /// </summary>
    public EventScheduleModel() { }

    /// <summary>
    /// Creates a new instance of an EventScheduleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public EventScheduleModel(Entities.EventSchedule entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.EventType = entity.EventType;
        this.IsEnabled = entity.IsEnabled;
        this.ScheduleId = entity.ScheduleId;
        this.Schedule = entity.Schedule != null ? new ScheduleModel(entity.Schedule) : null;
        this.NotificationId = entity.NotificationId;
        this.ReportId = entity.ReportId;
        this.FolderId = entity.FolderId;
        this.Settings = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>();
        this.RequestSentOn = entity.RequestSentOn;
        this.LastRanOn = entity.LastRanOn;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a EventSchedule object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.EventSchedule ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.EventSchedule)this;
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.EventSchedule(EventScheduleModel model)
    {
        var entity = new Entities.EventSchedule(model.Name, model.EventType, model.ScheduleId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            NotificationId = model.NotificationId,
            ReportId = model.ReportId,
            FolderId = model.FolderId,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            LastRanOn = model.LastRanOn,
            RequestSentOn = model.RequestSentOn,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
