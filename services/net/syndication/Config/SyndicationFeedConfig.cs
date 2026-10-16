using System.Text.Json;
using System.Text.Json.Serialization;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Models.Extensions;

namespace TNO.Services.Syndication.Config;

/// <summary>
/// SyndicationFeedConfig class, one feed URL in a syndication ingest's ordered 'feeds' configuration.
/// The position of the feed in the list is its priority when removing duplicate stories (first is highest).
/// </summary>
public class SyndicationFeedConfig
{
    #region Variables
    /// <summary>
    /// The ingest configuration key containing the ordered list of feeds.
    /// </summary>
    public const string FeedsKey = "feeds";

    /// <summary>
    /// The ingest configuration key containing a single feed URL (used when there are no 'feeds').
    /// </summary>
    public const string UrlKey = "url";
    #endregion

    #region Properties
    /// <summary>
    /// get/set - The feed URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    /// <summary>
    /// get/set - The source code to assign to stories from this feed.
    /// When empty the ingest's source mapping and source are used.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// get - The parsed feed URL.
    /// </summary>
    [JsonIgnore]
    public Uri? Uri { get; private set; }
    #endregion

    #region Methods
    /// <summary>
    /// Get the feeds configured for the ingest, in priority order.
    /// Uses the 'feeds' list when it contains a valid URL, otherwise the single 'url' value.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="invalid">Collects configuration values that were ignored because they are invalid.</param>
    /// <returns></returns>
    public static IReadOnlyList<SyndicationFeedConfig> FromIngest(IngestModel ingest, ICollection<string>? invalid = null)
    {
        var feeds = new List<SyndicationFeedConfig>();
        foreach (var feed in GetConfiguredFeeds(ingest, invalid))
        {
            var url = feed.Url?.Trim() ?? "";
            if (String.IsNullOrWhiteSpace(url)) continue;
            if (!TryCreateUri(url, out var uri))
            {
                invalid?.Add(url);
                continue;
            }

            var source = feed.Source?.Trim();
            feeds.Add(new SyndicationFeedConfig() { Url = url, Source = String.IsNullOrWhiteSpace(source) ? null : source, Uri = uri });
        }
        if (feeds.Count > 0) return feeds;

        var legacyUrl = ingest.Configuration.ContainsKey(UrlKey) ? GetString(ingest, UrlKey) : "";
        if (String.IsNullOrWhiteSpace(legacyUrl)) return feeds;
        if (!TryCreateUri(legacyUrl, out var legacyUri))
        {
            invalid?.Add(legacyUrl);
            return feeds;
        }

        feeds.Add(new SyndicationFeedConfig() { Url = legacyUrl, Uri = legacyUri });
        return feeds;
    }

    private static IEnumerable<SyndicationFeedConfig> GetConfiguredFeeds(IngestModel ingest, ICollection<string>? invalid)
    {
        if (!ingest.Configuration.ContainsKey(FeedsKey)) return [];
        try
        {
            return ingest.Configuration.GetDictionaryJsonValue<SyndicationFeedConfig[]>(FeedsKey) ?? [];
        }
        catch (Exception ex) when (ex is JsonException or InvalidCastException or FormatException or Core.Exceptions.ConfigurationException)
        {
            invalid?.Add(FeedsKey);
            return [];
        }
    }

    private static string GetString(IngestModel ingest, string key)
    {
        try
        {
            return ingest.GetConfigurationValue(key)?.Trim() ?? "";
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or Core.Exceptions.ConfigurationException)
        {
            return "";
        }
    }

    private static bool TryCreateUri(string url, out Uri? uri)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out uri);
    }
    #endregion
}
