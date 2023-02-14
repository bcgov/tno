using System;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.Ingest;

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
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Continuous;

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
    public int Repeat { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public ScheduleWeekDay RunOnWeekDays { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public ScheduleMonth RunOnMonths { get; set; }

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
            this.ScheduleType = entity.ScheduleType;
            this.DelayMS = entity.DelayMS;
            this.RunOn = entity.RunOn;
            this.StartAt = entity.StartAt;
            this.StopAt = entity.StopAt;
            this.RunOnlyOnce = entity.RunOnlyOnce;
            this.Repeat = entity.Repeat;
            this.RunOnWeekDays = entity.RunOnWeekDays;
            this.RunOnMonths = entity.RunOnMonths;
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
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Schedule(ScheduleModel model)
    {
        var entity = new Entities.Schedule(model.Name, model.ScheduleType, model.DelayMS)
        {
            Id = model.Id,
            Description = model.Description,
            RunOn = model.RunOn,
            IsEnabled = model.IsEnabled,
            StartAt = model.StartAt,
            StopAt = model.StopAt,
            RunOnlyOnce = model.RunOnlyOnce,
            Repeat = model.Repeat,
            RunOnWeekDays = model.RunOnWeekDays,
            RunOnMonths = model.RunOnMonths,
            DayOfMonth = model.DayOfMonth,
            RequestedById = model.RequestedById,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
