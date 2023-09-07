using TNO.API.Models;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.EventSchedule;

/// <summary>
/// ScheduleModel class, provides a model that represents an schedule.
/// </summary>
public class ScheduleModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to identify the schedule.
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
    public int DelayMS { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? RunOn { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public TimeSpan? StartAt { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public TimeSpan? StopAt { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool RunOnlyOnce { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool Repeat { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int[] RunOnWeekDays { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set -
    /// </summary>
    public int[] RunOnMonths { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set -
    /// </summary>
    public int DayOfMonth { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? RequestedById { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ScheduleModel.
    /// </summary>
    public ScheduleModel() { }

    /// <summary>
    /// Creates a new instance of an ScheduleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ScheduleModel(Schedule? entity) : base(entity)
    {
        if (entity != null)
        {
            this.Id = entity.Id;
            this.Name = entity.Name;
            this.Description = entity.Description;
            this.IsEnabled = entity.IsEnabled;
            this.DelayMS = entity.DelayMS;
            this.RunOn = entity.RunOn;
            this.StartAt = entity.StartAt;
            this.StopAt = entity.StopAt;
            this.RunOnlyOnce = entity.RunOnlyOnce;
            this.Repeat = entity.Repeat;
            this.RunOnWeekDays = entity.RunOnWeekDays.GetFlagValues();
            this.RunOnMonths = entity.RunOnMonths.GetFlagValues();
            this.DayOfMonth = entity.DayOfMonth;
            this.RequestedById = entity.RequestedById;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Ingest object.
    /// </summary>
    /// <returns></returns>
    public Entities.Schedule ToEntity()
    {
        var entity = (Entities.Schedule)this;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Schedule(ScheduleModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return new Schedule(model.Name, model.DelayMS)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            RunOnlyOnce = model.RunOnlyOnce,
            Repeat = model.Repeat,
            RunOn = model.RunOn,
            StartAt = model.StartAt,
            StopAt = model.StopAt,
            RunOnWeekDays = model.RunOnWeekDays.ToFlagEnum<ScheduleWeekDay>(),
            RunOnMonths = model.RunOnMonths.ToFlagEnum<ScheduleMonth>(),
            DayOfMonth = model.DayOfMonth,
            RequestedById = model.RequestedById,
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
