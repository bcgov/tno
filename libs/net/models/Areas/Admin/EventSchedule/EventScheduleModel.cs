using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.EventSchedule;

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
    /// get/set -
    /// </summary>
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

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
    public EventScheduleModel(Entities.EventSchedule entity) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.EventType = entity.EventType;
        this.IsEnabled = entity.IsEnabled;
        this.ScheduleId = entity.ScheduleId;
        this.Schedule = entity.Schedule != null ? new ScheduleModel(entity.Schedule) : null;
        this.Settings = entity.Settings;
        this.LastRanOn = entity.LastRanOn;
    }
    #endregion

    #region Methods
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
            Settings = model.Settings,
            LastRanOn = model.LastRanOn,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
