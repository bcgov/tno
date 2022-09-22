using TNO.API.Models;
using TNO.Entities;
using TNO.Core.Extensions;

namespace TNO.API.Areas.Services.Models.Ingest;

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
    public int ScheduleType { get; set; }

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
    public int Repeat { get; set; }

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
            this.ScheduleType = (int)entity.ScheduleType;
            this.DelayMS = entity.DelayMS;
            this.RunOn = entity.RunOn;
            this.StartAt = entity.StartAt;
            this.StopAt = entity.StopAt;
            this.Repeat = entity.Repeat;
            this.RunOnWeekDays = entity.RunOnWeekDays.GetFlagValues();
            this.RunOnMonths = entity.RunOnMonths.GetFlagValues();
            this.DayOfMonth = entity.DayOfMonth;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Schedule(ScheduleModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return new Schedule(model.Name, (ScheduleType)model.ScheduleType, model.DelayMS)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            Repeat = model.Repeat,
            RunOn = model.RunOn,
            StartAt = model.StartAt,
            StopAt = model.StopAt,
            RunOnWeekDays = model.RunOnWeekDays.ToFlagEnum<ScheduleWeekDay>(),
            RunOnMonths = model.RunOnMonths.ToFlagEnum<ScheduleMonth>(),
            DayOfMonth = model.DayOfMonth,
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
