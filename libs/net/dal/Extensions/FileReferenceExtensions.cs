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
        return Path.Combine(fileReference.Content?.GetStoragePath(context, storageConfig) ?? "", fileReference.Path);
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
        return fileReference.Content?.GetStoragePath(context, storageConfig) ?? "";
    }
}
