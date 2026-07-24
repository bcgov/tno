namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationScheduleModel class, represents the profile's scheduler event.
/// The scheduler service fires the event and requests an automation run; the automation
/// service itself performs no schedule evaluation (safe for horizontal scaling).
/// </summary>
public class AutomationScheduleModel
{
    #region Properties
    /// <summary>
    /// get/set - The event schedule id (0 when new).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - A name to identify the schedule.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Whether the schedule is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - The time of day the profile should run at (or after).
    /// </summary>
    public TimeSpan? StartAt { get; set; }

    /// <summary>
    /// get/set - The week days to run on (ScheduleWeekDay flag values).
    /// </summary>
    public int[] RunOnWeekDays { get; set; } = Array.Empty<int>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationScheduleModel.
    /// </summary>
    public AutomationScheduleModel() { }

    /// <summary>
    /// Creates a new instance of an AutomationScheduleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AutomationScheduleModel(Entities.EventSchedule entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.IsEnabled = entity.IsEnabled && (entity.Schedule?.IsEnabled ?? false);
        this.StartAt = entity.Schedule?.StartAt;
        this.RunOnWeekDays = entity.Schedule == null
            ? Array.Empty<int>()
            : Enum.GetValues<Entities.ScheduleWeekDay>()
                .Where(day => day != Entities.ScheduleWeekDay.NA && entity.Schedule.RunOnWeekDays.HasFlag(day))
                .Select(day => (int)day)
                .ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the combined ScheduleWeekDay flags value for the selected week days.
    /// </summary>
    /// <returns></returns>
    public Entities.ScheduleWeekDay GetRunOnWeekDays()
    {
        return this.RunOnWeekDays.Aggregate(Entities.ScheduleWeekDay.NA, (result, day) => result | (Entities.ScheduleWeekDay)day);
    }
    #endregion
}
