using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Extensions;

/// <summary>
/// DataSourceExtensions static class, provides extension methods for data sources.
/// </summary>
public static class DataSourceExtensions
{
    /// <summary>
    /// Update the context entity state.
    /// </summary>
    /// <param name="updated"></param>
    /// <param name="context"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static DataSource AddToContext(this DataSource updated, TNOContext context)
    {
        updated.ActionsManyToMany.SetEntityState(context);
        updated.MetricsManyToMany.SetEntityState(context);
        updated.SchedulesManyToMany.SetEntityState(context);
        return updated;
    }

    /// <summary>
    /// Update the data source in context.
    /// Provides a way to handle child collection changes.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="original"></param>
    /// <param name="updated"></param>
    /// <param name="updateChildren"></param>
    /// <returns></returns>
    public static TNOContext UpdateContext(this TNOContext context, DataSource original, DataSource updated, bool updateChildren = false)
    {
        if (updateChildren == true)
        {
            var oactions = context.DataSourceActions.Where(a => a.DataSourceId == updated.Id).ToArray();
            var ometrics = context.DataSourceMetrics.Where(m => m.DataSourceId == updated.Id).ToArray();
            var oschedules = context.DataSourceSchedules.Include(d => d.Schedule).Where(m => m.DataSourceId == updated.Id).ToArray();

            oactions.Except(updated.ActionsManyToMany).ForEach(a =>
            {
                context.Entry(a).State = EntityState.Deleted;
            });
            updated.ActionsManyToMany.ForEach(a =>
            {
                var current = oactions.FirstOrDefault(o => o.SourceActionId == a.SourceActionId);
                if (current == null)
                    original.ActionsManyToMany.Add(a);
                else if (current.Value != a.Value)
                {
                    current.Value = a.Value;
                    current.Version = a.Version;
                }
            });

            ometrics.Except(updated.MetricsManyToMany).ForEach(a =>
            {
                context.Entry(a).State = EntityState.Deleted;
            });
            updated.MetricsManyToMany.ForEach(a =>
            {
                var current = ometrics.FirstOrDefault(o => o.SourceMetricId == a.SourceMetricId);
                if (current == null)
                    original.MetricsManyToMany.Add(a);
                else if (current.Reach != a.Reach || current.Earned != a.Earned || current.Impression != a.Impression)
                {
                    current.Reach = a.Reach;
                    current.Earned = a.Earned;
                    current.Impression = a.Impression;
                    current.Version = a.Version;
                }
            });

            oschedules.Except(updated.SchedulesManyToMany).ForEach(a =>
            {
                context.Entry(a).State = EntityState.Deleted;
                if (a.Schedule != null)
                    context.Entry(a.Schedule).State = EntityState.Deleted;
            });
            updated.SchedulesManyToMany.ForEach(a =>
            {
                var current = oschedules.FirstOrDefault(o => o.ScheduleId == a.ScheduleId);
                if (current == null && a.Schedule != null)
                {
                    original.SchedulesManyToMany.Add(a);
                    if (a.Schedule.Id == 0)
                        context.Add(a.Schedule);
                    else if (a.Schedule != null)
                        context.Entry(a.Schedule).State = EntityState.Modified;

                }
                else if (current != null && current.Schedule != null && a.Schedule != null)
                {
                    current.Schedule.Name = a.Schedule.Name;
                    current.Schedule.Description = a.Schedule.Description;
                    current.Schedule.IsEnabled = a.Schedule.IsEnabled;
                    current.Schedule.ScheduleType = a.Schedule.ScheduleType;
                    current.Schedule.Repeat = a.Schedule.Repeat;
                    current.Schedule.DelayMS = a.Schedule.DelayMS;
                    current.Schedule.StartAt = a.Schedule.StartAt;
                    current.Schedule.StopAt = a.Schedule.StopAt;
                    current.Schedule.RunOn = a.Schedule.RunOn;
                    current.Schedule.RunOnWeekDays = a.Schedule.RunOnWeekDays;
                    current.Schedule.RunOnMonths = a.Schedule.RunOnMonths;
                    current.Schedule.DayOfMonth = a.Schedule.DayOfMonth;
                    current.Schedule.Version = a.Schedule.Version;
                }
            });
        }

        context.Entry(original).CurrentValues.SetValues(updated);
        context.ResetVersion(original);
        return context;
    }
}
