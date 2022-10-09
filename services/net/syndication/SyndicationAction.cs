using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Http;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.Actions;
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
    private readonly ILogger _logger;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka messenger.
    /// </summary>
    protected IKafkaMessenger Producer { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationAction, initializes with specified parameters.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="api"></param>
    /// <param name="producer"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SyndicationAction(IHttpRequestClient httpClient, IApiService api, IKafkaMessenger producer, IOptions<SyndicationOptions> options, ILogger<SyndicationAction> logger) : base(api, options)
    {
        _httpClient = httpClient;
        this.Producer = producer;
        _logger = logger;
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
    public override async Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogDebug("Performing ingestion service action for ingest '{name}'", manager.Ingest.Name);
        var url = GetUrl(manager.Ingest);

        var feed = await GetFeedAsync(url, manager);
        await ImportFeedAsync(manager, feed);
    }

    /// <summary>
    /// Iterate through feed and import content into api.
    /// Checks if a content reference has already been created for each item before deciding whether to import it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="feed"></param>
    /// <returns></returns>
    private async Task ImportFeedAsync(IIngestServiceActionManager manager, SyndicationFeed feed)
    {
        foreach (var item in feed.Items)
        {
            try
            {
                // Due to invalid RSS/ATOM need to use the link to identify the item.
                var link = item.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri;
                if (String.IsNullOrWhiteSpace(item.Id)) item.Id = link?.ToString() ?? throw new InvalidOperationException("Feed item requires a valid 'Id' or 'Link'.");

                // Fetch content reference.
                var reference = await this.Api.FindContentReferenceAsync(manager.Ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{manager.Ingest.Name}' is missing source code."), item.Id);
                var sendMessage = manager.Ingest.PostToKafka();
                if (reference == null)
                {
                    await FetchContent(manager.Ingest, item, link);
                    reference = await AddContentReferenceAsync(manager.Ingest, item);
                }
                else if ((reference.Status == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddHours(1) < DateTime.UtcNow) ||
                        (reference.Status != (int)WorkflowStatus.InProgress && item.PublishDate != DateTime.MinValue && reference.UpdatedOn != item.PublishDate.UtcDateTime))
                {
                    _logger.LogDebug("Updating existing content '{name}':{id}", manager.Ingest.Name, item.Id);
                    // TODO: verify a hash of the content to ensure it has changed. This may be slow
                    // however, but would ensure the content was physically updated.

                    // If another process has it in progress only attempt to do an import if it's
                    // more than an hour old. Assumption is that it is stuck.
                    await FetchContent(manager.Ingest, item, link);
                    reference = await UpdateContentReferenceAsync(reference, item);
                }
                else sendMessage = false;

                // Only send a message to Kafka if this process added/updated the content reference.
                if (reference != null && sendMessage)
                {
                    // Send item to Kafka.
                    var result = await this.Producer.SendMessageAsync(manager.Ingest.Topic, CreateSourceContent(manager.Ingest, item));

                    // Update content reference with Kafka response.
                    if (result != null)
                    {
                        await UpdateContentReferenceAsync(reference, result);
                    }
                }
            }
            catch (Exception ex)
            {
                // Reached limit return to ingest manager.
                if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                    throw;

                _logger.LogError(ex, "Failed to ingest item for ingest '{name}'", manager.Ingest.Name);
                await manager.RecordFailureAsync();
            }
        }
    }

    /// <summary>
    /// Make HTTP request to fetch story body only if configured to do so.
    /// Strip HTML from response body.
    /// Extract published on date and time.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="item"></param>
    /// <param name="link"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    private async Task FetchContent(IngestModel ingest, SyndicationItem item, Uri? link)
    {
        // Fetch content body separately if required
        if (ingest.GetConfigurationValue<bool>("fetchContent"))
        {
            if (link != null && Uri.IsWellFormedUriString(link.ToString(), UriKind.Absolute))
            {
                var html = await this.GetContentAsync(link);
                if (item.PublishDate == DateTime.MinValue) item.PublishDate = GetPubDateTime(html, ingest);
                var text = StringExtensions.SanitizeContent(html);
                // Only update the summary if it's empty.
                if (String.IsNullOrWhiteSpace(item.Summary.Text)) item.Summary = new TextSyndicationContent(text, TextSyndicationContentKind.Html);
                item.Content = new TextSyndicationContent(text, TextSyndicationContentKind.Html);
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
    /// <param name="url">The web location of a CP News article</param>
    /// <returns>An HTML formatted news article</returns>
    private async Task<string> GetContentAsync(Uri url)
    {
        var response = await _httpClient.GetAsync(url);
        var data = await response.Content.ReadAsStringAsync();

        return data;
    }

    /// <summary>
    /// Takes a CP News article and extracts the published date/time.
    /// If no date is found it will default to now.  This isn't ideal, but it's better than a min date.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="ingest"></param>
    /// <returns>A DateTime object representing the published date/time for the article</returns>
    private static DateTime GetPubDateTime(string content, IngestModel ingest)
    {
        var matches = Regex.Matches(content, "<DATE>(.+?)</DATE>");
        if (matches.Any())
        {
            var pubDate = matches[0].Groups[1].Value;
            var comps = pubDate.Split(' ');
            if (comps.Length == 3)
            {
                var timeZoneStr = ingest.GetConfigurationValue("timeZone");
                var timeZone = !string.IsNullOrEmpty(timeZoneStr) ?
                    TimeZoneInfo.FindSystemTimeZoneById(timeZoneStr) :
                    TimeZoneInfo.Local;
                var offset = timeZone.GetUtcOffset(DateTime.Now).Hours; // Handles daylight saving time
                var dateStr = $"{comps[1]} {comps[0]} {DateTime.Now.Year} {comps[2]} {offset}";
                return DateTime.ParseExact(dateStr, "dd MMM yyyy HH:mm z", CultureInfo.InvariantCulture);
            }
        }

        return DateTime.UtcNow;
    }

    /// <summary>
    /// Create a SourceContent object that can be sent to Kafka.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private SourceContent CreateSourceContent(IngestModel ingest, SyndicationItem item)
    {
        var (title, summary, body) = HandleInvalidEncoding(item);
        var source = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code.");
        var contentType = ingest.IngestType?.ContentType ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing ingest content type.");
        return new SourceContent(source, contentType, ingest.ProductId, item.Id, title, summary, body, item.PublishDate.UtcDateTime)
        {
            Link = item.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri.ToString() ?? "",
            Copyright = item.Copyright?.Text ?? "",
            Language = "", // TODO: Need to extract this from the ingest, or determine it after transcription.
            Authors = item.Authors.Select(a => new Author(a.Name, a.Email, a.Uri)),
            UpdatedOn = item.LastUpdatedTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null,
            Tags = item.Categories.Select(c => new TNO.Kafka.Models.Tag(c.Name, c.Label))
        };
    }

    /// <summary>
    /// Handle invalid encoding characters before sending to Kafka.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>updated title, summary, body</returns>
    private (string title, string summary, string body) HandleInvalidEncoding(SyndicationItem item)
    {
        var title = item.Title.Text;
        var summary = item.Summary.Text;
        var content = item.Content as TextSyndicationContent;
        var body = content?.Text ?? item.Content?.ToString() ?? "";
        if (!string.IsNullOrEmpty(this.Options.InvalidEncodings) && this.Options.EncodingSets != null)
        {
            foreach (var encodingSet in this.Options.EncodingSets)
            {
                var keyValue = encodingSet.Split(":_", StringSplitOptions.RemoveEmptyEntries);
                if (keyValue?.Length == 2)
                {
                    var oldValue = keyValue[0];
                    var newValue = keyValue[1];
                    if (title.Contains(oldValue)) title = title.Replace(oldValue, newValue);
                    if (summary.Contains(oldValue)) summary = summary.Replace(oldValue, newValue);
                    if (body.Contains(oldValue)) body = body.Replace(oldValue, newValue);
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
    /// <returns></returns>
    private async Task<ContentReferenceModel?> AddContentReferenceAsync(IngestModel ingest, SyndicationItem item)
    {
        // Add a content reference record.
        return await this.Api.AddContentReferenceAsync(new ContentReferenceModel()
        {
            Source = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code."),
            Uid = item.Id,
            Topic = ingest.Topic,
            Status = (int)WorkflowStatus.InProgress,
            PublishedOn = item.PublishDate != DateTime.MinValue ? item.PublishDate.UtcDateTime : null,
            SourceUpdateOn = item.LastUpdatedTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null,
        });
    }

    /// <summary>
    /// Send AJAX request to api to update content reference.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel reference, SyndicationItem item)
    {
        reference.PublishedOn = item.PublishDate != DateTime.MinValue ? item.PublishDate.UtcDateTime : null;
        reference.SourceUpdateOn = item.LastUpdatedTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null;
        return await UpdateContentReferenceAsync(reference);
    }

    /// <summary>
    /// Make AJAX request to fetch syndication feed.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="manager"></param>
    /// <returns></returns>
    private async Task<SyndicationFeed> GetFeedAsync(Uri url, IIngestServiceActionManager manager)
    {
        var response = await _httpClient.GetAsync(url);
        var data = await response.Content.ReadAsStringAsync();

        try
        {
            // Reformat dates because timezone abbreviations don't work...

            var xmlr = XmlReader.Create(new StringReader(data));
            var feed = SyndicationFeed.Load(xmlr);

            // Need to manually test if `pubDate` is valid.  Microsoft didn't bother providing a way to work with valid dates...
            // Essentially timezone values are context sensitive.
            var pubDate = feed.Items.FirstOrDefault()?.PublishDate;

            return feed;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Syndication feed for ingest '{name}' is invalid.", manager.Ingest.Name);

            var settings = new XmlReaderSettings()
            {
                IgnoreComments = false,
                IgnoreWhitespace = true,
            };
            var xmlr = XmlReader.Create(new StringReader(data), settings);
            var document = XDocument.Load(xmlr);
            var isRss = RssFeed.IsRssFeed(document);
            return isRss ? RssFeed.Load(document, false) : AtomFeed.Load(document);
        }
    }

    /// <summary>
    /// Extract the URL from the ingest connection settings.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Uri GetUrl(IngestModel ingest)
    {
        if (!ingest.Configuration.TryGetValue("url", out object? element)) throw new InvalidOperationException("Data source connection information is missing 'url'.");

        var value = (JsonElement)element;
        if (value.ValueKind == JsonValueKind.String)
        {
            var url = value.GetString() ?? throw new InvalidOperationException("Data source connection 'url' cannot be null, empty or whitespace.");

            var options = new UriCreationOptions();
            if (!Uri.TryCreate(url, options, out Uri? uri)) throw new InvalidOperationException("Data source connection 'url' is not a valid format.");

            return uri;
        }

        throw new InvalidOperationException("Data source connection 'url' is not a valid string value");
    }
    #endregion
}
