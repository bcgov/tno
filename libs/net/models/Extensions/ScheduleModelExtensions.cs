using TNO.API.Areas.Services.Models.DataSource;

namespace TNO.Models.Extensions;

/// <summary>
/// ScheduleModelExtensions static class, provies extension methods for ScheduleModel.
/// </summary>
public static class ScheduleModelExtensions
{
    /// <summary>
    /// Calculate the duraction of the schedule.
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public static TimeSpan CalcDuraction(this ScheduleModel schedule)
    {
        if (schedule.StartAt == null || schedule.StopAt == null) return new TimeSpan();
        var startAt = schedule.StartAt.Value;
        var stopAt = schedule.StopAt.Value;
        return stopAt.Subtract(startAt);
    }
}
