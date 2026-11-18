namespace TNO.API.Helpers;

/// <summary>
/// Resolves storage paths inside a configured local directory before accessing the filesystem.
/// </summary>
public static class StoragePath
{
    /// <summary>
    /// Resolve a path within the storage root, rejecting traversal and symbolic links below it.
    /// </summary>
    /// <param name="rootPath">The trusted storage directory from server configuration.</param>
    /// <param name="path">A relative path or an absolute path within the configured root.</param>
    /// <returns>The normalized path within the root.</returns>
    public static string Resolve(string rootPath, string path)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var fullPath = Path.GetFullPath(Path.Combine(root, path));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (fullPath.Equals(root, comparison)) return root;
        var prefix = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(prefix, comparison))
            throw new ArgumentException("The path must stay within the storage directory.", nameof(path));

        // Do not let an existing link redirect a later read, write, or directory creation outside the root.
        var current = fullPath;
        while (!current.Equals(root, comparison))
        {
            if (new FileInfo(current).LinkTarget != null)
                throw new ArgumentException("Symbolic links are not supported in storage paths.", nameof(path));
            current = Path.GetDirectoryName(current)!;
        }
        return fullPath;
    }

    /// <summary>
    /// Validate a filename supplied separately from its storage directory.
    /// </summary>
    /// <param name="name">The uploaded filename or generated clip name.</param>
    /// <returns>The filename, when it is a single nonempty path component.</returns>
    public static string FileName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Contains("..") || name.Contains('/') || name.Contains('\\') || name.Contains(':') ||
            name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("A filename must be a single path component.", nameof(name));
        return name;
    }
}
