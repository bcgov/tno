using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Models.Filters;
using TNO.Services.Actions;
using TNO.Services.Dedup;
using TNO.Services.Syndication.Config;
using TNO.Services.Syndication.Xml;

namespace TNO.Services.Syndication;

/// <summary>
/// SyndicationAction class, performs the syndication ingestion action.
/// Fetch syndication feed.
/// Send content reference to API.
/// Send message to Kafka.
/// Update content reference status.
/// </summary>
public class SyndicationAction : IngestAction<SyndicationOptions>
{
    #region Variables
    private readonly IHttpRequestClient _httpClient;
    private readonly object _lock = new object();
    private string? _etag;
    private readonly ConcurrentDictionary<string, int?> _sourceMediaTypeMap = new ConcurrentDictionary<string, int?>();

    /// <summary>
    /// Stories recently ingested by this ingest, used to skip duplicates across its feeds.
    /// An action is created per ingest, so the index belongs to one ingest.
    /// </summary>
    private readonly DedupIndex _dedupIndex = new();
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationAction, initializes with specified parameters.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SyndicationAction(IHttpRequestClient httpClient, IApiService api, IOptions<SyndicationOptions> options, ILogger<SyndicationAction> logger) : base(api, options, logger)
    {
        _httpClient = httpClient;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<ServiceActionResult> PerformActionAsync<T>(IIngestActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        var ingest = manager.Ingest;
        this.Logger.LogDebug("Performing ingestion service action for ingest '{name}'", ingest.Name);

        // This ingest has just begun running.
        await manager.UpdateIngestStateFailedAttemptsAsync(ingest.FailedAttempts);
        await GetSourcesConfigurationAsync();

        var invalid = new List<string>();
        var feeds = SyndicationFeedConfig.FromIngest(ingest, invalid);
        foreach (var value in invalid)
            this.Logger.LogWarning("Ingest '{name}' has an invalid feed configuration value '{value}'", ingest.Name, value);
        if (feeds.Count == 0) throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing a valid feed 'url'.");

        var dedup = GetDedupOptions(ingest);
        if (dedup.Enabled) await LoadDedupIndexAsync(ingest, feeds, dedup);

        // Feeds are imported in priority order, so a story is kept from the highest priority feed that has it.
        var result = new FeedImportResult();
        ExceptionDispatchInfo? firstFailure = null;
        for (var priority = 0; priority < feeds.Count; priority++)
        {
            var feedConfig = feeds[priority];
            try
            {
                var feed = await GetFeedAsync(feedConfig.Uri!, manager);
                await ImportFeedAsync(manager, feed, feedConfig, priority, dedup, result);
                result.Feeds++;
            }
            catch (Exception ex) when (feeds.Count > 1)
            {
                // One unavailable feed must not stop the others.
                firstFailure ??= ExceptionDispatchInfo.Capture(ex);
                this.Logger.LogError(ex, "Failed to import feed {position} of {count} '{url}' for ingest '{name}'", priority + 1, feeds.Count, feedConfig.Url, ingest.Name);
            }
            finally
            {
                // This ingest has just completed running for one feed.
                await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);
            }
        }

        this.Logger.LogDebug("Ingest '{name}' imported {feeds} of {count} feeds, {stories} new stories, {duplicates} duplicates skipped", ingest.Name, result.Feeds, feeds.Count, result.Stories, result.Duplicates);

        // When every feed fails the run has failed, the same as a single feed failing.
        if (result.Feeds == 0) firstFailure?.Throw();

        return ServiceActionResult.Success;
    }

    /// <summary>
    /// Iterate through feed and import content into api.
    /// Checks if a content reference has already been created for each item before deciding whether to import it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="feed"></param>
    /// <param name="feedConfig">The configuration of the feed.</param>
    /// <param name="priority">The position of the feed in the ingest's feeds, lower is more important.</param>
    /// <param name="dedup">How to find duplicate stories.</param>
    /// <param name="result">Counts of what was imported.</param>
    /// <returns></returns>
    private async Task ImportFeedAsync(IIngestActionManager manager, SyndicationFeed feed, SyndicationFeedConfig feedConfig, int priority, DedupOptions dedup, FeedImportResult result)
    {
        foreach (var item in feed.Items)
        {
            try
            {
                // Due to invalid RSS/ATOM need to use the link to identify the item.
                var link = item.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri;
                if (String.IsNullOrWhiteSpace(item.Id)) item.Id = link?.ToString() ?? throw new InvalidOperationException("Feed item requires a valid 'Id' or 'Link'.");

                // Some feeds require a separate request to fetch relevant meta-data.
                await FetchContent(manager, item, link);
                var sourceContent = CreateSourceContent(manager.Ingest, item, feedConfig.Source);

                // Fetch content reference.
                var reference = await this.FindContentReferenceAsync(sourceContent.Source, sourceContent.Uid);
                if (reference == null)
                {
                    // A new story, skip it if an equal or higher priority feed already has it.
                    DedupEntry? entry = null;
                    if (dedup.Enabled)
                    {
                        entry = CreateDedupEntry(sourceContent, priority, dedup);
                        if (_dedupIndex.TryFindMatch(entry, dedup, out var match) && match != null)
                        {
                            result.Duplicates++;
                            LogDuplicate(manager.Ingest, entry, sourceContent, match);
                            continue;
                        }
                    }

                    reference = await AddContentReferenceAsync(manager.Ingest, item, sourceContent, entry);
                    if (reference != null)
                    {
                        result.Stories++;
                        if (entry != null) _dedupIndex.Add(entry);
                    }
                }
                // If the Content Service hasn't imported within X minutes, then perhaps send another message.
                else if ((reference.Status.In((int)WorkflowStatus.Failed, (int)WorkflowStatus.InProgress, (int)WorkflowStatus.Received)
                            && reference.UpdatedOn?.AddSeconds(this.Options.RetryAfterSeconds) < DateTime.UtcNow) ||
                        (reference.Status.In((int)WorkflowStatus.Imported) // They've updated the story, so we need to update our copy.
                            && item.PublishDate.UtcDateTime != DateTime.MinValue
                            && (reference.PublishedOn != item.PublishDate.UtcDateTime
                                || (item.LastUpdatedTime.UtcDateTime != DateTime.MinValue
                                    && reference.SourceUpdateOn != item.LastUpdatedTime.UtcDateTime))))
                {
                    // TODO: verify a hash of the content to ensure it has changed. This may be slow
                    // however, but would ensure the content was physically updated.

                    // If another process has it in progress only attempt to do an import if it's
                    // more than an hour old. Assumption is that it is stuck.
                    this.Logger.LogWarning("Updating content {source}:{uid}", reference.Source, reference.Uid);
                    reference = await this.UpdateContentReferenceAsync(reference, item);
                }
                else continue;

                await ContentReceivedAsync(manager, reference, item, sourceContent);
            }
            catch (Exception ex)
            {
                // Reached limit return to ingest manager.
                if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                    throw;

                this.Logger.LogError(ex, "Failed to ingest item for ingest '{name}'", manager.Ingest.Name);
                await manager.RecordFailureAsync(ex);
                await manager.SendEmailAsync($"Failed to ingest item for ingest '{manager.Ingest.Name}'", ex);
            }
        }
    }

    /// <summary>
    /// Create the duplicate detection fingerprint for a story.
    /// </summary>
    /// <param name="sourceContent"></param>
    /// <param name="priority"></param>
    /// <param name="dedup"></param>
    /// <returns></returns>
    private static DedupEntry CreateDedupEntry(SourceContent sourceContent, int priority, DedupOptions dedup)
    {
        var body = !String.IsNullOrWhiteSpace(sourceContent.Body) ? sourceContent.Body : sourceContent.Summary;
        return DedupEntry.Create(sourceContent.Source, sourceContent.Uid, sourceContent.Title, body, sourceContent.PublishedOn ?? DateTime.UtcNow, priority, dedup);
    }

    /// <summary>
    /// Log a skipped duplicate, at information level the first time and debug after that.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="entry"></param>
    /// <param name="sourceContent"></param>
    /// <param name="match"></param>
    private void LogDuplicate(IngestModel ingest, DedupEntry entry, SourceContent sourceContent, DedupMatch match)
    {
        var level = _dedupIndex.MarkSkipped(entry) ? LogLevel.Information : LogLevel.Debug;
        this.Logger.Log(level,
            "Ingest '{name}' skipped duplicate story {source}:{uid} '{headline}', it matches {primarySource}:{primaryUid} ({reason} {score:0.00})",
            ingest.Name, entry.Source, entry.Uid, sourceContent.Title, match.Primary.Source, match.Primary.Uid, match.Reason, match.Score);
    }

    /// <summary>
    /// Get the duplicate detection settings from the ingest configuration, using the service defaults for any that are missing or invalid.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private DedupOptions GetDedupOptions(IngestModel ingest)
    {
        return new DedupOptions()
        {
            Enabled = GetConfigurationBoolean(ingest, "dedupEnabled"),
            WindowDays = (int)GetConfigurationNumber(ingest, "dedupWindowDays", this.Options.DedupWindowDays, 1, 90),
            HeadlineThreshold = GetConfigurationNumber(ingest, "dedupHeadlineThreshold", this.Options.DedupHeadlineThreshold, 0.01, 1),
            BodyThreshold = GetConfigurationNumber(ingest, "dedupBodyThreshold", this.Options.DedupBodyThreshold, 0.5, 1),
            MinHeadlineTokens = this.Options.DedupMinHeadlineTokens,
            MinBodyTokens = this.Options.DedupMinBodyTokens,
        };
    }

    /// <summary>
    /// Load the duplicate index with the stories this ingest's sources published within the window.
    /// The index is reloaded when the ingest configuration changes and periodically, otherwise old stories are pruned.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="feeds"></param>
    /// <param name="dedup"></param>
    /// <returns></returns>
    private async Task LoadDedupIndexAsync(IngestModel ingest, IReadOnlyList<SyndicationFeedConfig> feeds, DedupOptions dedup)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(-dedup.WindowDays);
        if (_dedupIndex.Version == ingest.Version && _dedupIndex.LoadedOn?.AddMinutes(this.Options.DedupReloadMinutes) > now)
        {
            _dedupIndex.Prune(cutoff);
            return;
        }

        var priorities = GetSourcePriorities(ingest, feeds);
        var entries = new List<DedupEntry>();
        var quantity = Math.Max(this.Options.DedupLoadPageSize, 1);
        for (var page = 1; page <= Math.Max(this.Options.DedupLoadMaxPages, 1); page++)
        {
            var filter = new ContentReferenceFilter()
            {
                // Only this ingest's stories, other ingests may use the same sources.
                Topic = ingest.Topic,
                Sources = priorities.Keys.ToArray(),
                PublishedStartOn = cutoff,
                Page = page,
                Quantity = quantity,
            };
            var references = (await this.Api.FindContentReferencesAsync(filter)).ToArray();
            foreach (var reference in references)
            {
                // Only references created with duplicate detection have a fingerprint.
                if (priorities.TryGetValue(reference.Source, out var priority) && DedupMetadata.TryRead(reference, priority, out var entry) && entry != null)
                    entries.Add(entry);
            }
            if (references.Length < quantity) break;
        }

        _dedupIndex.Load(entries, ingest.Version, now);
        this.Logger.LogInformation("Ingest '{name}' loaded {count} stories from the last {days} days into its duplicate index", ingest.Name, entries.Count, dedup.WindowDays);
    }

    /// <summary>
    /// Map each source code this ingest can produce to the priority of the first feed that produces it.
    /// Feeds without a source produce the ingest's source and the sources in its source mapping.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="feeds"></param>
    /// <returns></returns>
    private static Dictionary<string, int> GetSourcePriorities(IngestModel ingest, IReadOnlyList<SyndicationFeedConfig> feeds)
    {
        var priorities = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var priority = 0; priority < feeds.Count; priority++)
        {
            var source = feeds[priority].Source;
            if (!String.IsNullOrWhiteSpace(source))
            {
                priorities.TryAdd(source, priority);
                continue;
            }

            if (!String.IsNullOrWhiteSpace(ingest.Source?.Code)) priorities.TryAdd(ingest.Source.Code, priority);
            foreach (var mapping in GetSourceMappings(ingest).Where(m => !String.IsNullOrWhiteSpace(m.Value)))
                priorities.TryAdd(mapping.Value, priority);
        }
        return priorities;
    }

    /// <summary>
    /// Read a boolean ingest configuration value, false if it is missing or invalid.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private static bool GetConfigurationBoolean(IngestModel ingest, string key)
    {
        if (!ingest.Configuration.TryGetValue(key, out var value) || value is not JsonElement element) return false;
        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.String => Boolean.TryParse(element.GetString(), out var result) && result,
            _ => false,
        };
    }

    /// <summary>
    /// Read a numeric ingest configuration value.
    /// Returns the default value when it is missing, not a number, or outside the range.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    private static double GetConfigurationNumber(IngestModel ingest, string key, double defaultValue, double min, double max)
    {
        if (!ingest.Configuration.TryGetValue(key, out var value) || value is not JsonElement element) return defaultValue;
        double number;
        if (element.ValueKind == JsonValueKind.Number) number = element.GetDouble();
        else if (element.ValueKind == JsonValueKind.String && Double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)) number = parsed;
        else return defaultValue;
        return number < min || number > max ? defaultValue : number;
    }

    /// <summary>
    /// Make a request to the API for all the source configurations.
    /// Store their mediaTypeId override value in memory.
    /// </summary>
    /// <returns></returns>
    private async Task GetSourcesConfigurationAsync()
    {
        try
        {
            // Fetch the latest configuration values.
            var response = String.IsNullOrWhiteSpace(_etag) ? await this.Api.GetSourcesResponseAsync() : await this.Api.GetSourcesResponseWithEtagAsync(_etag);
            var etag = this.Api.GetResponseEtag(response);
            lock (_lock)
            {
                _etag = etag;
            }
            var sources = await this.Api.GetResponseDataAsync<SourceModel[]>(response);
            if (sources != null)
            {
                foreach (var source in sources)
                {
                    _sourceMediaTypeMap.AddOrUpdate(source.Code, code => source.MediaTypeId, (code, oldId) => source.MediaTypeId);
                }
                var current = _sourceMediaTypeMap.Keys.ToArray() ?? [];
                var latest = sources.Select(s => s.Code).ToArray();
                var removeSources = current.Where(code => !latest.Contains(code));
                foreach (var source in removeSources)
                {
                    // Remove any sources that no longer exist.
                    _sourceMediaTypeMap.Remove(source, out int? value);
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to fetch sources");
        }
    }

    /// <summary>
    /// Parse the date time value and handle a common formatting issues.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="value"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    private DateTimeOffset ParseDateTime(IngestModel ingest, string value, string? format = null)
    {
        if (String.IsNullOrWhiteSpace(format))
            return value.Length == 12 ? ParseDateTime(ingest, value, "MMM dd HH:mm") : DateTimeOffset.Parse(value);
        else
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sourcePublishDate))
            {
                var sourceTimeZoneId = ingest.GetConfigurationValue("timeZone");
                return !string.IsNullOrEmpty(sourceTimeZoneId) ?
                    TimeZoneInfo.ConvertTimeBySystemTimeZoneId(sourcePublishDate, sourceTimeZoneId, TimeZoneInfo.Local.Id) :
                    sourcePublishDate;
            }

            return DateTimeOffset.ParseExact(value, format, CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Make HTTP request to fetch story body only if configured to do so.
    /// Strip HTML from response body.
    /// Extract published on date and time.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="item"></param>
    /// <param name="link"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    private async Task FetchContent(IIngestActionManager manager, SyndicationItem item, Uri? link)
    {
        var ingest = manager.Ingest;
        // Fetch content body separately if required
        if (ingest.GetConfigurationValue<bool>("fetchContent"))
        {
            if (link != null && Uri.IsWellFormedUriString(link.ToString(), UriKind.Absolute))
            {
                var text = await this.GetContentAsync(manager, link);
                var html = new HtmlDocument();
                html.LoadHtml(text);
                var dateNode = html.DocumentNode.SelectSingleNode("//html/p/date");
                if (dateNode != null)
                    item.PublishDate = ParseDateTime(ingest, dateNode.InnerText);

                var articleNode = html.DocumentNode.SelectSingleNode("//html/article");
                if (articleNode != null)
                {
                    var bylineNode = articleNode.SelectSingleNode("p[starts-with(., 'By ')]");
                    if (bylineNode != null)
                    {
                        item.Authors.Add(new SyndicationPerson { Name = bylineNode.InnerHtml.Replace("By ", "").Replace("\n", "") });
                        bylineNode.Remove();
                    }
                    var content = new TextSyndicationContent(
                        articleNode.InnerHtml.Replace("\r\n", " ").Replace("\n", " "),
                        TextSyndicationContentKind.Html);
                    // Only update the summary if it's empty.
                    if (string.IsNullOrWhiteSpace(item.Summary.Text)) item.Summary = content;
                    item.Content = content;
                }
                else
                {
                    var content = new TextSyndicationContent(StringExtensions.ConvertTextToParagraphs(text, @"[\r\n]+"), TextSyndicationContentKind.Html);
                    // Only update the summary if it's empty.
                    if (string.IsNullOrWhiteSpace(item.Summary.Text)) item.Summary = content;
                    item.Content = content;
                }
            }
            else
            {
                throw new HttpRequestException($"Invalid URL for content body: {link}");
            }
        }
    }

    /// <summary>
    /// Make an AJAX request to fetch the CP News article identified by url.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="url">The web location of a CP News article</param>
    /// <returns>An HTML formatted news article</returns>
    private async Task<string> GetContentAsync(IIngestActionManager manager, Uri url)
    {
        var response = await _httpClient.SendAsync(url, HttpMethod.Get, ApplyCredentials(manager), null);
        if (!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
            this.Logger.LogError(ex, "Failed to fetch syndication feed content for ingest '{name}' - {url}", manager.Ingest.Name, url);
            throw ex;
        }
        else
        {
            var data = await response.Content.ReadAsStringAsync();
            return data;
        }
    }

    /// <summary>
    /// If the ingest has source mappings configured, find a match and apply the source to the item.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string DetermineSource(IngestModel ingest, SyndicationItem item)
    {
        var sourceValues = GetSourceMappings(ingest);

        var defaultSource = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code.");
        var itemSource = item.SourceFeed?.Title?.Text;

        // Find a match if possible
        if (!String.IsNullOrEmpty(itemSource))
        {
            var sourceMatch = sourceValues.FirstOrDefault(s => s.Key.Equals(itemSource, StringComparison.InvariantCultureIgnoreCase)).Value;
            if (!String.IsNullOrWhiteSpace(sourceMatch)) return sourceMatch;
        }
        return defaultSource;
    }

    /// <summary>
    /// Get the source mapping configuration ('name=code&amp;name=code'), which maps a feed item's source name to a source code.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private static KeyValuePair<string, string>[] GetSourceMappings(IngestModel ingest)
    {
        return ingest.Configuration.GetDictionaryJsonValue<string>("sources")?.Split('&').Select(v =>
        {
            var keyValue = v.Split('=');
            if (keyValue.Length > 1) return new KeyValuePair<string, string>(keyValue[0], keyValue[1]);
            return new KeyValuePair<string, string>(keyValue[0], keyValue[0]);
        }).ToArray() ?? [];
    }

    /// <summary>
    /// Determine the media type for the specified ingest and source.
    /// If the source has an override, use it.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    private int DetermineMediaType(IngestModel ingest, string source)
    {
        _sourceMediaTypeMap.TryGetValue(source, out var mediaTypeId);
        return mediaTypeId ?? ingest.MediaTypeId;
    }

    /// <summary>
    /// Create a SourceContent object that can be sent to Kafka.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="item"></param>
    /// <param name="feedSource">The source code configured for the feed, which overrides the ingest's source mapping.</param>
    /// <returns></returns>
    private SourceContent CreateSourceContent(IngestModel ingest, SyndicationItem item, string? feedSource = null)
    {
        var (title, summary, body) = HandleInvalidEncoding(item);
        var source = !String.IsNullOrWhiteSpace(feedSource) ? feedSource : DetermineSource(ingest, item);
        var contentType = ingest.IngestType?.ContentType ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing ingest content type.");
        var publishedOn = item.PublishDate.UtcDateTime != DateTime.MinValue ? item.PublishDate.UtcDateTime : (DateTime?)null;
        var uid = Runners.BaseService.GetContentHash(source, title, publishedOn);
        var mediaTypeId = DetermineMediaType(ingest, source);
        string? media = null;

        // Extract values from namespaces.  I don't know a better way to do this.
        foreach (var ext in item.ElementExtensions)
        {
            if (ext.GetObject<XElement>().Name.LocalName == "encoded") // content:encoded
                body = ext.GetObject<XElement>().Value;
            if (ext.GetObject<XElement>().Name.LocalName == "creator") // dc:creator
                item.Authors.Add(new SyndicationPerson(null, ext.GetObject<XElement>().Value, null));
            if (ext.GetObject<XElement>().Name.LocalName == "content") // media:content
                media = ext.GetObject<XElement>().Attributes("url").FirstOrDefault()?.Value;
        }

        return new SourceContent(
            this.Options.DataLocation,
            source,
            contentType,
            mediaTypeId,
            uid,
            title,
            StringExtensions.ConvertTextToParagraphs(summary, @"[\r\n]+"),
            StringExtensions.ConvertTextToParagraphs(body, @"[\r\n]+"),
            publishedOn)
        {
            Link = item.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri.ToString() ?? "",
            Copyright = item.Copyright?.Text ?? "",
            Language = "", // TODO: Need to extract this from the ingest, or determine it after transcription.
            Authors = item.Authors.Select(a => new Author(a.Name, a.Email, a.Uri)),
            UpdatedOn = item.LastUpdatedTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null,
            Labels = item.Categories.Select(c => new LabelModel(c.Label, c.Name)),
            ExternalUid = item.Id,
        };
    }

    /// <summary>
    /// Handle invalid encoding characters before sending to Kafka.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>updated title, summary, body</returns>
    private (string title, string summary, string body) HandleInvalidEncoding(SyndicationItem item)
    {
        var title = item.Title.Text ?? "";
        var summary = item.Summary.Text ?? "";
        var content = item.Content as TextSyndicationContent;
        bool hasToStringOverride = false;
        if (item.Content != null)
        {
            hasToStringOverride = item.Content?.GetType().ToString() != item.Content?.ToString();
        }
        var body = content?.Text ?? (hasToStringOverride ? item.Content?.ToString() : "") ?? "";
        if (!string.IsNullOrEmpty(this.Options.InvalidEncodings) && this.Options.EncodingSets != null)
        {
            foreach (var encodingSet in this.Options.EncodingSets)
            {
                var keyValue = encodingSet.Split(":_", StringSplitOptions.RemoveEmptyEntries);
                if (keyValue?.Length == 2)
                {
                    var oldValue = keyValue[0];
                    var newValue = keyValue[1];
                    if (title.Contains(oldValue) == true) title = title.Replace(oldValue, newValue);
                    if (summary.Contains(oldValue) == true) summary = summary.Replace(oldValue, newValue);
                    if (body.Contains(oldValue) == true) body = body.Replace(oldValue, newValue);
                }
            }
        }
        return (title, summary, body);
    }

    /// <summary>
    /// Send AJAX request to api to add content reference.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="item"></param>
    /// <param name="sourceContent"></param>
    /// <param name="entry">The duplicate detection fingerprint to store with the reference.</param>
    /// <returns></returns>
    private async Task<ContentReferenceModel?> AddContentReferenceAsync(IngestModel ingest, SyndicationItem item, SourceContent sourceContent, DedupEntry? entry = null)
    {
        // Add a content reference record.
        var reference = CreateContentReference(ingest, item, sourceContent);
        if (entry != null) DedupMetadata.Write(reference.Metadata, entry);
        return await this.Api.AddContentReferenceAsync(reference);
    }

    /// <summary>
    /// Creates a ContentReferenceModel for a syndication item
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="item"></param>
    /// <param name="sourceContent"></param>
    /// <returns></returns>
    private static ContentReferenceModel CreateContentReference(IngestModel ingest, SyndicationItem item, SourceContent sourceContent)
    {
        return new ContentReferenceModel()
        {
            Source = sourceContent.Source,
            Uid = sourceContent.Uid,
            Topic = ingest.Topic,
            Status = (int)WorkflowStatus.InProgress,
            PublishedOn = sourceContent.PublishedOn,
            SourceUpdateOn = item.LastUpdatedTime.UtcDateTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null,
            Metadata = new Dictionary<string, object?> {
                { ContentReferenceMetaDataKeys.IngestSource, ingest.Source!.Code }
            }
        };
    }

    /// <summary>
    /// Send AJAX request to api to update content reference.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel reference, SyndicationItem item)
    {
        reference.PublishedOn = item.PublishDate.UtcDateTime != DateTime.MinValue ? item.PublishDate.UtcDateTime : null;
        reference.SourceUpdateOn = item.LastUpdatedTime.UtcDateTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null;
        return await UpdateContentReferenceAsync(reference, WorkflowStatus.InProgress);
    }

    /// <summary>
    /// Send AJAX request to api to update content reference.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="reference"></param>
    /// <param name="item"></param>
    /// <param name="sourceContent"></param>
    /// <returns></returns>
    private async Task<ContentReferenceModel?> ContentReceivedAsync(IIngestActionManager manager, ContentReferenceModel? reference, SyndicationItem item, SourceContent sourceContent)
    {
        if (reference != null)
        {
            reference.PublishedOn = item.PublishDate.UtcDateTime != DateTime.MinValue ? item.PublishDate.UtcDateTime : null;
            reference.SourceUpdateOn = item.LastUpdatedTime.UtcDateTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null;
            return await ContentReceivedAsync(manager, reference, sourceContent);
        }
        return reference;
    }

    /// <summary>
    /// Apply the authentication header if required.
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="manager"></param>
    private static HttpRequestHeaders ApplyCredentials(IIngestActionManager manager, HttpRequestHeaders? headers = null)
    {
        if (headers == null) headers = new HttpRequestMessage().Headers;
        var username = manager.Ingest.Configuration.GetDictionaryJsonValue<string>("username");
        var password = manager.Ingest.Configuration.GetDictionaryJsonValue<string>("password");

        if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
        {
            var encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"));
            var auth = new AuthenticationHeaderValue("Basic", encoded);
            headers.Authorization = auth;
        }
        return headers;
    }

    /// <summary>
    /// Make AJAX request to fetch syndication feed.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="manager"></param>
    /// <returns></returns>
    private async Task<SyndicationFeed> GetFeedAsync(Uri url, IIngestActionManager manager)
    {
        var response = await _httpClient.SendAsync(url, HttpMethod.Get, ApplyCredentials(manager), null);
        if (!response.IsSuccessStatusCode)
        {
            var ex = new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
            this.Logger.LogError(ex, "Failed to fetch syndication feed for ingest '{name}'", manager.Ingest.Name);
            throw ex;
        }

        var data = await response.Content.ReadAsStringAsync() ?? "";
        if (!manager.Ingest.Configuration.GetDictionaryJsonValue("customFeed", false))
        {
            try
            {
                var settings = new XmlReaderSettings()
                {
                    IgnoreComments = false,
                    IgnoreWhitespace = true,
                    DtdProcessing = DtdProcessing.Parse,
                };
                using var xmlr = XmlReader.Create(new StringReader(data), settings);
                var feed = SyndicationFeed.Load(xmlr);
                return feed;
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning(ex, "Syndication feed for ingest '{name}' is invalid.", manager.Ingest.Name);
                return GetCustomFeed(data);
            }
        }
        else
        {
            return GetCustomFeed(data);
        }
    }

    /// <summary>
    /// Make AJAX request to fetch syndication feed.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static SyndicationFeed GetCustomFeed(string data)
    {
        var settings = new XmlReaderSettings()
        {
            IgnoreComments = false,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Parse,
        };
        var xmlr = XmlReader.Create(new StringReader(data), settings);
        var document = XDocument.Load(xmlr);
        var isRss = RssFeed.IsRssFeed(document);
        return isRss ? RssFeed.Load(document, false) : AtomFeed.Load(document);
    }
    #endregion

    /// <summary>
    /// FeedImportResult class, counts what one run imported.
    /// </summary>
    private sealed class FeedImportResult
    {
        public int Feeds { get; set; }
        public int Stories { get; set; }
        public int Duplicates { get; set; }
    }
}
