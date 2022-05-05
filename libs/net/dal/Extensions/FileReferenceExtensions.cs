using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.Entities;

namespace TNO.DAL.Extensions;

/// <summary>
/// FileReferenceExtensions static class, provides extension methods for FileReference objects.
/// </summary>
public static class FileReferenceExtensions
{
    /// <summary>
    /// Determine the full path to the file.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="context"></param>
    /// <param name="storageConfig"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetFilePath(this IReadonlyFileReference fileReference, TNOContext context, StorageConfig storageConfig)
    {
        return Path.Combine(fileReference.GetStoragePath(context, storageConfig), fileReference.Path);
    }

    /// <summary>
    /// Determine the full path to the file for the specified 'fileReference'.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="context"></param>
    /// <param name="storageConfig"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetStoragePath(this IReadonlyFileReference fileReference, TNOContext context, StorageConfig storageConfig)
    {
        // Determine the DataLocation that will be used to store the file.
        var source = fileReference.Content?.DataSource?.Code ?? fileReference.Content?.Source;
        if (String.IsNullOrWhiteSpace(source) && fileReference.Content?.DataSourceId != null) source = context.DataSources.Find(fileReference.Content?.DataSourceId)?.Code;

        DataLocation location;
        if (fileReference.Content?.DataSource?.DataLocationId != null)
            location = context.DataLocations.Find(fileReference.Content.DataSource.DataLocationId) ?? throw new ArgumentException("DataLocation does not exist");
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
        var path = Path.Combine(storageConfig.GetPath(), connectionPath);
        return path.EndsWith('/') ? path : $"{path}/";
    }
}
