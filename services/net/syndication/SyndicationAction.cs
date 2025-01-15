using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
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
        this.Logger.LogDebug("Performing ingestion service action for ingest '{name}'", manager.Ingest.Name);

        // This ingest has just begun running.
        await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);

        var url = GetUrl(manager.Ingest);

        var feed = await GetFeedAsync(url, manager);
        await ImportFeedAsync(manager, feed);

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
    /// <returns></returns>
    private async Task ImportFeedAsync(IIngestActionManager manager, SyndicationFeed feed)
    {
        foreach (var item in feed.Items)
        {
            try
            {
                // var nsm = new XmlNamespaceManager(xmlr.NameTable);
                // nsm.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
                // nsm.AddNamespace("wfw", "http://wellformedweb.org/CommentAPI/");
                // nsm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
                // nsm.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                // nsm.AddNamespace("sy", "http://purl.org/rss/1.0/modules/syndication/");
                // nsm.AddNamespace("slash", "http://purl.org/rss/1.0/modules/slash/");

                // Due to invalid RSS/ATOM need to use the link to identify the item.
                var link = item.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri;
                if (String.IsNullOrWhiteSpace(item.Id)) item.Id = link?.ToString() ?? throw new InvalidOperationException("Feed item requires a valid 'Id' or 'Link'.");

                // Some feeds require a separate request to fetch relevant meta-data.
                await FetchContent(manager, item, link);
                var sourceContent = CreateSourceContent(manager.Ingest, item);

                // Fetch content reference.
                var reference = await this.FindContentReferenceAsync(manager.Ingest.Source?.Code, sourceContent.Uid);
                if (reference == null)
                {
                    reference = await AddContentReferenceAsync(manager.Ingest, item, sourceContent);
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
            finally
            {
                // This ingest has just completed running for one content item.
                await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);
            }
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
        var publishedOn = item.PublishDate.UtcDateTime != DateTime.MinValue ? item.PublishDate.UtcDateTime : (DateTime?)null;
        var uid = Runners.BaseService.GetContentHash(source, title, publishedOn);
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
            ingest.MediaTypeId,
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
    /// <returns></returns>
    private async Task<ContentReferenceModel?> AddContentReferenceAsync(IngestModel ingest, SyndicationItem item, SourceContent sourceContent)
    {
        // Add a content reference record.
        return await this.Api.AddContentReferenceAsync(CreateContentReference(ingest, item, sourceContent));
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
                using var xmlr = XmlReader.Create(new StringReader(data));
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
