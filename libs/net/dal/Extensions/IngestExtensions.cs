using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Extensions;

/// <summary>
/// IngestExtensions static class, provides extension methods for ingest configurations.
/// </summary>
public static class IngestExtensions
{
    /// <summary>
    /// Update the context entity state.
    /// </summary>
    /// <param name="updated"></param>
    /// <returns></returns>
    public static Ingest AddToContext(this Ingest updated)
    {
        updated.Schedules.AddRange(updated.SchedulesManyToMany.Select(s => s.Schedule!).Where(s => s != null).ToArray());
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
    public static TNOContext UpdateContext(this TNOContext context, Ingest original, Ingest updated, bool updateChildren = false)
    {
        if (updateChildren == true)
        {
            var oschedules = context.IngestSchedules.Include(d => d.Schedule).Where(m => m.IngestId == updated.Id).ToArray();
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

            var oDataLocations = context.IngestDataLocations.Include(d => d.DataLocation).Where(m => m.IngestId == updated.Id).ToArray();
            oDataLocations.Except(updated.DataLocationsManyToMany).ForEach(a =>
            {
                context.Entry(a).State = EntityState.Deleted;
            });
            updated.DataLocationsManyToMany.ForEach(a =>
            {
                var current = oDataLocations.FirstOrDefault(o => o.DataLocationId == a.DataLocationId);
                if (current == null) original.DataLocationsManyToMany.Add(a);
            });

            if (original.State != null && updated.State != null)
                context.Entry(original.State).CurrentValues.SetValues(updated.State);
        }

        context.Entry(original).CurrentValues.SetValues(updated);
        context.ResetVersion(original);
        return context;
    }
}
