using System.Text.Json;
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
            var current = a.ActionId != 0 ? oactions.FirstOrDefault(o => o.ActionId == a.ActionId) : null;
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
            var current = a.CategoryId != 0 ? ocategories.FirstOrDefault(o => o.CategoryId == a.CategoryId) : null;
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
            var current = a.TonePoolId != 0 ? otonepools.FirstOrDefault(o => o.TonePoolId == a.TonePoolId) : null;
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
            var current = a.Id != 0 ? otimetrackings.FirstOrDefault(o => o.Id == a.Id) : null;
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
        ofilereferences.Except(updated.FileReferences).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.FileReferences.ForEach(a =>
        {
            var current = a.Id != 0 ? ofilereferences.FirstOrDefault(o => o.Id == a.Id) : null;
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

        olinks.Except(updated.Links).ForEach(a =>
        {
            context.Entry(a).State = EntityState.Deleted;
        });
        updated.Links.ForEach(a =>
        {
            var current = a.LinkId != 0 ? olinks.FirstOrDefault(o => o.LinkId == a.LinkId) : null;
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
    /// <param name="storageConfig"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetFilePath(this Content content, string path, TNOContext context, StorageConfig storageConfig)
    {
        return Path.Combine(content.GetStoragePath(context, storageConfig), path);
    }

    /// <summary>
    /// Determine the full path to the file for the specified 'content' including the configured storage location.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="context"></param>
    /// <param name="storageConfig"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetStoragePath(this Content content, TNOContext context, StorageConfig storageConfig)
    {
        // Determine the DataLocation that will be used to store the file.
        var source = content.DataSource?.Code ?? content.Source;
        if (String.IsNullOrWhiteSpace(source) && content.DataSourceId != null) source = context.DataSources.Find(content.DataSourceId)?.Code;

        DataLocation location;
        if (content.DataSource?.DataLocationId != null)
            location = context.DataLocations.Find(content.DataSource.DataLocationId) ?? throw new ArgumentException("DataLocation does not exist");
        else if (!String.IsNullOrWhiteSpace(source)) // This isn't an ideal situation, as it means the user can loosely link the datasource with the content.
            location = context.DataSources
                .Include(ds => ds.DataLocation)
                .FirstOrDefault(ds => ds.Code.ToLower() == source.ToLower())?.DataLocation
                ?? context.DataLocations.FirstOrDefault(dl => dl.Name == "Default") ?? throw new ArgumentException("The 'Default' DataLocation has not been configured");
        else
            location = context.DataLocations.FirstOrDefault(dl => dl.Name == "Default") ?? throw new ArgumentException("The 'Default' DataLocation has not been configured");

        // TODO: Handle different data locations.
        // TODO: Handle when the original file uploaded has different path than the new one.
        var dataConnection = JsonSerializer.Deserialize<Dictionary<string, object>>(location.Connection) ?? new Dictionary<string, object>();
        var connectionPath = dataConnection.ContainsKey("path") ? $"{((string?)dataConnection["path"])?.RemoveStartAndEnd("/")}" : "";
        var path = Path.Combine(storageConfig.GetUploadPath(), connectionPath);
        return path.EndsWith('/') ? path : $"{path}/";
    }
}
