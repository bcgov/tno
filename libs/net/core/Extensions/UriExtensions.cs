namespace TNO.Core.Extensions;

using System;
using System.Linq;

/// <summary>
/// UriExtensions static class, provides extension methods for Uri objects.
/// </summary>
public static class UriExtensions
{
    /// <summary>
    /// Append paths to the specified 'uri'.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static Uri Append(this Uri uri, params string[] paths)
    {
        return new Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))));
    }
}
