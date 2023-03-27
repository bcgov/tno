using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Extensions;

/// <summary>
/// SourceExtensions static class, provides extension methods for sources.
/// </summary>
public static class SourceExtensions
{
    /// <summary>
    /// Update the context entity state.
    /// </summary>
    /// <param name="updated"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Source AddToContext(this Source updated, TNOContext context)
    {
        updated.MetricsManyToMany.SetEntityState(context);
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
    public static TNOContext UpdateContext(this TNOContext context, Source original, Source updated, bool updateChildren = false)
    {
        if (updateChildren == true)
        {
            var ometrics = context.SourceMetrics.Where(m => m.SourceId == updated.Id).ToArray();

            ometrics.Except(updated.MetricsManyToMany).ForEach(a =>
            {
                context.Entry(a).State = EntityState.Deleted;
            });
            updated.MetricsManyToMany.ForEach(a =>
            {
                var current = ometrics.FirstOrDefault(o => o.MetricId == a.MetricId);
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
        }

        context.Entry(original).CurrentValues.SetValues(updated);
        context.ResetVersion(original);
        return context;
    }
}
