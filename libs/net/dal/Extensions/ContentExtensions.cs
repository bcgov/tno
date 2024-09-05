using System.Web;
using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.Entities;

namespace TNO.DAL.Extensions;

/// <summary>
/// ContentExtensions static class, provides extension methods for content.
/// </summary>
public static class ContentExtensions
{
    /// <summary>
    /// Ensures that the specified content has a 'Uid'.
    /// If it doesn't it generates one from either the 'SourceUrl', or the 'Id'.
    /// </summary>
    /// <param name="content"></param>
    /// <returns>True if a new 'Uid' was generated.</returns>
    public static bool GuaranteeUid(this Content content)
    {
        // Generate a UID for all content.
        if (String.IsNullOrWhiteSpace(content.Uid))
        {
            if (!String.IsNullOrWhiteSpace(content.SourceUrl) && Uri.TryCreate(content.SourceUrl, UriKind.Absolute, out Uri? uri))
            {
                var builder = new UriBuilder(uri!);
                var query = HttpUtility.ParseQueryString(builder.Query);
                query["_id"] = content.Id.ToString();
                builder.Query = query.ToString();
                content.Uid = builder.ToString();
            }
            else
            {
                content.Uid = $"tno-{content.Id:00000000}";
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Update the context entity state so that it and related entities are added to the database.
    /// </summary>
    /// <param name="updated"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Content AddToContext(this Content updated, TNOContext context)
    {
        updated.ActionsManyToMany.SetEntityState(context);
        updated.TopicsManyToMany.SetEntityState(context);
        updated.TagsManyToMany.SetEntityState(context);
        updated.Labels.SetEntityState(context);
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

    /// <summary>
    /// Update the context entity state so that it and related entities are updated in the database.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="original"></param>
    /// <param name="updated"></param>
    /// <returns></returns>
    public static TNOContext UpdateContext(this TNOContext context, Content original, Content updated)
    {
        var oActions = context.ContentActions.Where(a => a.ContentId == updated.Id).ToArray();
        var oTopics = context.ContentTopics.Where(a => a.ContentId == updated.Id).ToArray();
        var oTags = context.ContentTags.Where(a => a.ContentId == updated.Id).ToArray();
        var oTonePools = context.ContentTonePools.Where(a => a.ContentId == updated.Id).ToArray();
        var oTimeTrackings = context.TimeTrackings.Where(a => a.ContentId == updated.Id).ToArray();
        var oFileReferences = context.FileReferences.Where(a => a.ContentId == updated.Id).ToArray();
        var oLinks = context.ContentLinks.Where(a => a.ContentId == updated.Id).ToArray();
        var oLabels = context.ContentLabels.Where(a => a.ContentId == updated.Id).ToArray();
        var oQuotes = context.Quotes.Where(a => a.ContentId == updated.Id).ToArray();

        oActions.Except(updated.ActionsManyToMany).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.ActionsManyToMany.ForEach(a =>
        {
            var current = a.ActionId != 0 ? oActions.FirstOrDefault(o => o.ActionId == a.ActionId) : null;
            if (current == null)
                original.ActionsManyToMany.Add(a);
            else if (current.Value != a.Value)
            {
                current.Value = a.Value;
                current.Version = a.Version;
            }
        });

        // We don't presently remove any quotes.
        // oQuotes.Except(updated.Quotes).ForEach(a =>
        // {
        //     context.Entry(a).State = EntityState.Deleted;
        // });
        updated.Quotes.ForEach(a =>
        {
            var current = a.Id != 0 ? oQuotes.FirstOrDefault(o => o.Id == a.Id) : null;
            if (current == null)
                original.Quotes.Add(a);
            else if (current.Byline != a.Byline || current.Statement != a.Statement || current.IsRelevant != a.IsRelevant)
            {
                current.Byline = a.Byline;
                current.Statement = a.Statement;
                current.IsRelevant = a.IsRelevant;
                current.Version = a.Version;
            }
        });

        oTopics.Except(updated.TopicsManyToMany).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.TopicsManyToMany.ForEach(a =>
        {
            var current = a.TopicId != 0 ? oTopics.FirstOrDefault(o => o.TopicId == a.TopicId) : null;
            if (current == null)
                original.TopicsManyToMany.Add(a);
            else if (current.Score != a.Score)
            {
                current.Score = a.Score;
                current.Version = a.Version;
            }
        });

        oTags.Except(updated.TagsManyToMany).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.TagsManyToMany.ForEach(a =>
        {
            var current = oTags.FirstOrDefault(o => o.TagId == a.TagId);
            if (current == null)
                original.TagsManyToMany.Add(a);
        });

        oLabels.Except(updated.Labels).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.Labels.ForEach(a =>
        {
            var current = a.Id != 0 ? oLabels.FirstOrDefault(o => o.Id == a.Id) : null;
            if (current == null)
                original.Labels.Add(a);
            else if (current.Key != a.Key ||
                current.Value != a.Value)
            {
                current.Key = a.Key;
                current.Value = a.Value;
                current.Version = a.Version;
            }
        });

        updated.TonePoolsManyToMany.ForEach(a =>
        {
            var current = a.TonePoolId != 0 ? oTonePools.FirstOrDefault(o => o.TonePoolId == a.TonePoolId) : null;
            
            // If no matching tone pool exists, add it to the original list
            if (current == null)
            {
                a.TonePool ??= context.TonePools.FirstOrDefault(tp => tp.Id == a.TonePoolId);
                
                if (a.TonePool != null &&
                    !original.TonePoolsManyToMany.Any(x => x.TonePool != null && x.TonePool.Id == a.TonePool.Id))
                {
                    original.TonePoolsManyToMany.Add(a);
                }
            }
            // If a matching tone pool is found but the values have changed, update the values
            else if (current.Value != a.Value)
            {
                current.Value = a.Value;
                current.Version = a.Version;
            }
        });

        oTimeTrackings.Except(updated.TimeTrackings).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.TimeTrackings.ForEach(a =>
        {
            var current = a.Id != 0 ? oTimeTrackings.FirstOrDefault(o => o.Id == a.Id) : null;
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

        // While the DB supports multiple files, presently the intent is only a single file for content.
        oFileReferences.Except(updated.FileReferences).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.FileReferences.ForEach(a =>
        {
            var current = a.Id != 0 ? oFileReferences.FirstOrDefault(o => o.Id == a.Id) : null;
            if (current == null)
                original.FileReferences.Add(a);
            else if (current.ContentType != a.ContentType ||
                current.FileName != a.FileName ||
                current.Size != a.Size ||
                current.RunningTime != a.RunningTime)
            {
                current.ContentType = a.ContentType;
                current.FileName = a.FileName;
                current.Size = a.Size;
                current.RunningTime = a.RunningTime;
                current.Version = a.Version;
            }
        });

        oLinks.Except(updated.Links).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.Links.ForEach(a =>
        {
            var current = a.LinkId != 0 ? oLinks.FirstOrDefault(o => o.LinkId == a.LinkId) : null;
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

    /// <summary>
    /// Determine the full path to the file including the configured storage location.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="path">The path to the file not including the storage location.</param>
    /// <param name="context"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetFilePath(this Content content, string path, TNOContext context, StorageOptions options)
    {
        return Path.Combine(content.GetStoragePath(context, options), path);
    }

    /// <summary>
    /// Determine the full path to the file for the specified 'content' including the configured storage location.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetStoragePath(this Content content, TNOContext context, StorageOptions options)
    {
        var path = options.GetUploadPath();
        return path.EndsWith('/') ? path : $"{path}/";
    }
}
