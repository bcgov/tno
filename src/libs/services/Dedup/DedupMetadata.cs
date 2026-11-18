using System.Globalization;
using System.Text.Json;
using TNO.API.Areas.Services.Models.ContentReference;

namespace TNO.Services.Dedup;

/// <summary>
/// DedupMetadata static class, stores a story's fingerprint in its content reference metadata
/// so the index can be rebuilt after a restart.
/// </summary>
public static class DedupMetadata
{
    #region Methods
    /// <summary>
    /// Write the fingerprint of the entry into the metadata.
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="entry"></param>
    public static void Write(IDictionary<string, object?> metadata, DedupEntry entry)
    {
        metadata[ContentReferenceMetaDataKeys.DedupHeadline] = entry.Headline;
        if (entry.BodyHash.HasValue)
            metadata[ContentReferenceMetaDataKeys.DedupBodyHash] = entry.BodyHash.Value.ToString("x16", CultureInfo.InvariantCulture);
        else
            metadata.Remove(ContentReferenceMetaDataKeys.DedupBodyHash);
    }

    /// <summary>
    /// Read the fingerprint stored in a content reference.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="priority">The priority of the feed the story came from.</param>
    /// <param name="entry"></param>
    /// <returns>False if the reference has no fingerprint.</returns>
    public static bool TryRead(ContentReferenceModel reference, int priority, out DedupEntry? entry)
    {
        entry = null;
        var headline = GetString(reference.Metadata, ContentReferenceMetaDataKeys.DedupHeadline);
        if (String.IsNullOrWhiteSpace(headline)) return false;

        var hashText = GetString(reference.Metadata, ContentReferenceMetaDataKeys.DedupBodyHash);
        ulong? bodyHash = !String.IsNullOrWhiteSpace(hashText) && UInt64.TryParse(hashText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hash) ? hash : null;
        var publishedOn = reference.PublishedOn ?? reference.CreatedOn ?? DateTime.UtcNow;

        entry = new DedupEntry(reference.Source, reference.Uid, headline, bodyHash, publishedOn, priority);
        return true;
    }

    /// <summary>
    /// Metadata values are strings when created in memory and JsonElements when returned by the API.
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private static string? GetString(IReadOnlyDictionary<string, object?> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var value) || value == null) return null;
        return value switch
        {
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => null,
        };
    }
    #endregion
}
