using System.Collections.Generic;
using System.Linq;

namespace TNO.CSS.Extensions;

/// <summary>
/// DictionaryExtensions static class, provides extension methods for the attributes dictionary values.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Extract the username from the attributes.
    /// </summary>
    /// <param name="attributes"></param>
    /// <returns></returns>
    public static string? GetUsername(this Dictionary<string, string[]> attributes)
    {
        if (attributes.TryGetValue("idir_username", out string[]? idir)) return idir?.FirstOrDefault();
        if (attributes.TryGetValue("github_username", out string[]? github)) return github?.FirstOrDefault();
        if (attributes.TryGetValue("bceid_username", out string[]? bcied)) return bcied?.FirstOrDefault();
        return null;
    }
}
