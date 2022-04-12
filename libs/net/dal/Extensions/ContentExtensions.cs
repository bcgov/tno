using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Extensions;

/// <summary>
/// ContentExtensions static class, provides extension methods for content.
/// </summary>
public static class ContentExtensions
{
    /// <summary>
    /// Update the context entity state.
    /// </summary>
    /// <param name="updated"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Content AddToContext(this Content updated, TNOContext context)
    {
        updated.ActionsManyToMany.SetEntityState(context);
        updated.CategoriesManyToMany.SetEntityState(context);
        updated.TagsManyToMany.SetEntityState(context);
        updated.TonePoolsManyToMany.SetEntityState(context);
        updated.TimeTrackings.SetEntityState(context);
        updated.FileReferences.SetEntityState(context);
        updated.Links.SetEntityState(context);
        if (updated.Series?.Id == 0)
        {
            context.Entry(updated.Series).State = EntityState.Added;
            updated.SeriesId = 0;
        }
        return updated;
    }

    public static TNOContext UpdateContext(this TNOContext context, Content original, Content updated)
    {
        var oactions = context.ContentActions.Where(a => a.ContentId == updated.Id).ToArray();
        var ocategories = context.ContentCategories.Where(a => a.ContentId == updated.Id).ToArray();
        var otags = context.ContentTags.Where(a => a.ContentId == updated.Id).ToArray();
        var otonepools = context.ContentTonePools.Where(a => a.ContentId == updated.Id).ToArray();
        var otimetrackings = context.TimeTrackings.Where(a => a.ContentId == updated.Id).ToArray();
        var ofilereferences = context.FileReferences.Where(a => a.ContentId == updated.Id).ToArray();
        var olinks = context.ContentLinks.Where(a => a.ContentId == updated.Id).ToArray();

        oactions.Except(updated.ActionsManyToMany).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.ActionsManyToMany.ForEach(a =>
        {
            var current = oactions.FirstOrDefault(o => o.ActionId == a.ActionId);
            if (current == null)
                original.ActionsManyToMany.Add(a);
            else if (current.Value != a.Value)
            {
                current.Value = a.Value;
                current.Version = a.Version;
            }
        });

        ocategories.Except(updated.CategoriesManyToMany).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.CategoriesManyToMany.ForEach(a =>
        {
            var current = ocategories.FirstOrDefault(o => o.CategoryId == a.CategoryId);
            if (current == null)
                original.CategoriesManyToMany.Add(a);
            else if (current.Score != a.Score)
            {
                current.Score = a.Score;
                current.Version = a.Version;
            }
        });

        otags.Except(updated.TagsManyToMany).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.TagsManyToMany.ForEach(a =>
        {
            var current = otags.FirstOrDefault(o => o.TagId == a.TagId);
            if (current == null)
                original.TagsManyToMany.Add(a);
        });

        otonepools.Except(updated.TonePoolsManyToMany).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.TonePoolsManyToMany.ForEach(a =>
        {
            var current = otonepools.FirstOrDefault(o => o.TonePoolId == a.TonePoolId);
            if (current == null)
                original.TonePoolsManyToMany.Add(a);
            else if (current.Value != a.Value)
            {
                current.Value = a.Value;
                current.Version = a.Version;
            }
        });

        otimetrackings.Except(updated.TimeTrackings).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.TimeTrackings.ForEach(a =>
        {
            var current = otimetrackings.FirstOrDefault(o => o.Id == a.Id);
            if (current == null)
                original.TimeTrackings.Add(a);
            else if (current.UserId != a.UserId || current.Effort != a.Effort || current.Activity != a.Activity)
            {
                current.UserId = a.UserId;
                current.Effort = a.Effort;
                current.Activity = a.Activity;
                current.Version = a.Version;
            }
        });

        ofilereferences.Except(updated.FileReferences).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.FileReferences.ForEach(a =>
        {
            var current = ofilereferences.FirstOrDefault(o => o.Id == a.Id);
            if (current == null)
                original.FileReferences.Add(a);
            else if (current.MimeType != a.MimeType || current.Path != a.Path || current.Size != a.Size || current.RunningTime != a.RunningTime)
            {
                current.MimeType = a.MimeType;
                current.Path = a.Path;
                current.Size = a.Size;
                current.RunningTime = a.RunningTime;
                current.Version = a.Version;
            }
        });

        olinks.Except(updated.Links).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.Links.ForEach(a =>
        {
            var current = olinks.FirstOrDefault(o => o.LinkId == a.LinkId);
            if (current == null)
                original.Links.Add(a);
        });

        context.Entry(original).CurrentValues.SetValues(updated);

        if (updated.Series?.Id == 0)
        {
            // Dynamically add a new series.  This could result in an error if the name is not unique.
            context.Entry(updated.Series).State = EntityState.Added;
            original.SeriesId = updated.SeriesId;
            original.Series = updated.Series;
        }
        context.ResetVersion(original);
        return context;
    }
}
