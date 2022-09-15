using TNO.API.Areas.Services.Models.Ingest;

namespace TNO.Models.Extensions;

/// <summary>
/// ScheduleModelExtensions static class, provies extension methods for ScheduleModel.
/// </summary>
public static class ScheduleModelExtensions
{
    /// <summary>
    /// Calculate the duration of the schedule.
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public static TimeSpan CalcDuration(this ScheduleModel schedule)
    {
        if (schedule.StartAt == null || schedule.StopAt == null) return new TimeSpan();
        var startAt = schedule.StartAt.Value;
        var stopAt = schedule.StopAt.Value;
        return stopAt.Subtract(startAt);
    }
}
