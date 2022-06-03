using System;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Core.Http;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Models.Kafka;
using TNO.Services.Actions;
using TNO.Services.CPNews.Config;
using TNO.Services.Models.Xml;
using System.Security.Cryptography;

namespace TNO.Services.CPNews;

/// <summary>
/// CPNewsAction class, performs the cpnews ingestion action.
/// Fetch cpnews feed.
/// Send message to Kafka.
/// Inform api of new content.
/// </summary>
public class CPNewsAction : IngestAction<CPNewsOptions>
{
    #region Variables
    private readonly IHttpRequestClient _httpClient;
    private readonly IKafkaMessenger _kafka;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CPNewsAction, initializes with specified parameters.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="api"></param>
    /// <param name="kafka"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CPNewsAction(IHttpRequestClient httpClient, IApiService api, IKafkaMessenger kafka, IOptions<CPNewsOptions> options, ILogger<CPNewsAction> logger) : base(api, options)
    {
        _httpClient = httpClient;
        _kafka = kafka;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task PerformActionAsync(IDataSourceIngestManager manager, string? name = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Performing ingestion service action for data source '{Code}'", manager.DataSource.Code);
        var url = GetUrl(manager.DataSource);

        var feed = await GetFeedAsync(url, manager);
        await ImportFeedAsync(manager, feed);
    }

    /// <summary>
    /// Iterate through feed and import content into api.
    /// Checks if a content reference has already been created for each item before deciding whether to import it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="feed"></param>
    /// <returns></returns>
    private async Task ImportFeedAsync(IDataSourceIngestManager dataSource, SyndicationFeed feed)
    {
        foreach (var item in feed.Items)
        {
            try
            {
                // Fetch content reference.
                var link = item.Links.Count > 0 ? item.Links[0].Uri.AbsoluteUri : "";
                var reference = await this.Api.FindContentReferenceAsync(dataSource.DataSource.Code, link);
                var sendMessage = true;
                var articleContent = await this.GetContentAsync(link);

                item.Id = link;
                articleContent = StringExtensions.SanitizeContent(articleContent);
                item.Summary = new TextSyndicationContent(articleContent);

                if (reference == null)
                {
                    reference = await AddContentReferenceAsync(dataSource.DataSource, item);
                }
                else if ((reference.WorkflowStatus == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddHours(1) < DateTime.UtcNow) ||
                        (reference.WorkflowStatus != (int)WorkflowStatus.InProgress && reference.UpdatedOn != item.PublishDate.UtcDateTime))
                {
                    // TODO: verify a hash of the content to ensure it has changed. This may be slow
                    // however, but would ensure the content was physically updated.

                    // If another process has it in progress only attempt to do an import if it's
                    // more than an hour old. Assumption is that it is stuck.
                    reference = await UpdateContentReferenceAsync(reference, item);
                }
                else sendMessage = false;

                // Only send a message to Kafka if this process added/updated the content reference.
                if (sendMessage && reference != null)
                {
                    // Send item to Kafka.
                    var content = CreateSourceContent(dataSource.DataSource.Code, item);
                    var result = await _kafka.SendMessageAsync(dataSource.DataSource.Topic, content);

                    // Update content reference with Kafka response.
                    if (result != null)
                        await UpdateContentReferenceAsync(reference, result);
                }
            }
            catch (Exception ex)
            {
                // Reached limit return to data source manager.
                if (dataSource.DataSource.FailedAttempts + 1 >= dataSource.DataSource.RetryLimit)
                    throw;

                _logger.LogError(ex, "Failed to ingest item for data source '{Code}'", dataSource.DataSource.Code);
                await dataSource.RecordFailureAsync();
            }
        }
    }

    /// <summary>
    /// Create a SourceContent object that can be sent to Kafka.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private static SourceContent CreateSourceContent(string source, SyndicationItem item)
    {
        return new SourceContent(SourceMediaType.Text, source, item.Id, item.Title.Text, item.Summary.Text, item.Content?.ToString() ?? "", item.PublishDate.UtcDateTime)
        {
            Link = item.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri.ToString() ?? "",
            Copyright = item.Copyright?.Text ?? "",
            Language = "", // TODO: Need to extract this from the data source, or determine it after transcription.
            Authors = item.Authors.Select(a => new Author(a.Name, a.Email, a.Uri)),
            UpdatedOn = item.LastUpdatedTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null,
            Tags = item.Categories.Select(c => new TNO.Models.Kafka.Tag(c.Name, c.Label))
        };
    }

    /// <summary>
    /// Send AJAX request to api to add content reference.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private async Task<ContentReferenceModel?> AddContentReferenceAsync(DataSourceModel dataSource, SyndicationItem item)
    {
        // Add a content reference record.
        return await this.Api.AddContentReferenceAsync(new ContentReferenceModel()
        {
            Source = dataSource.Code,
            Uid = item.Id,
            Topic = dataSource.Topic,
            WorkflowStatus = (int)WorkflowStatus.InProgress,
            PublishedOn = null, // item.PublishDate != DateTime.MinValue ? item.PublishDate.UtcDateTime : null,
            SourceUpdateOn = null // item.LastUpdatedTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null,
        });
    }

    /// <summary>
    /// Send AJAX request to api to update content reference.
    /// This content reference has been successfully recieved by Kafka.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel reference, DeliveryResult<string, SourceContent> result)
    {
        reference.Offset = result.Offset;
        reference.Partition = result.Partition;
        reference.WorkflowStatus = (int)WorkflowStatus.Received;
        return await this.Api.UpdateContentReferenceAsync(reference);
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
        return await this.Api.UpdateContentReferenceAsync(reference);
    }

    /// <summary>
    /// Make AJAX request to fetch cpnews feed.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="manager"></param>
    /// <returns></returns>
    private async Task<SyndicationFeed> GetFeedAsync(Uri url, IDataSourceIngestManager manager)
    {
        var response = await _httpClient.GetAsync(url);
        var data = await response.Content.ReadAsStringAsync();

        try
        {
            var xmlr = XmlReader.Create(new StringReader(data));
            return SyndicationFeed.Load(xmlr);

            // var rss = new Rss20FeedFormatter();
            // rss.ReadFrom(xmlr);
            // var feed = rss.Feed;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "CPNews feed for data source '{Code}' is invalid.", manager.DataSource.Code);

            var settings = new XmlReaderSettings()
            {
                IgnoreComments = false,
                IgnoreWhitespace = true,
            };
            var xmlr = XmlReader.Create(new StringReader(data), settings);
            // var rss = RssFeed.Load(xmlr);
            var document = XDocument.Load(xmlr);
            var isRss = RssFeed.IsRssFeed(document);
            return isRss ? RssFeed.Load(document, false) : AtomFeed.Load(document);
        }
    }

    /// <summary>
    /// Make an AJAX request to fetch the CP News article identified by url.
    /// </summary>
    /// <param name="url">The web location of a CP News article</param>
    /// <returns>An HTML formatted news article</returns>
    private async Task<string> GetContentAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        var data = await response.Content.ReadAsStringAsync();

        return data;
    }

    /// <summary>
    /// Extract the URL from the data source connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Uri GetUrl(DataSourceModel dataSource)
    {
        if (!dataSource.Connection.TryGetValue("url", out object? element)) throw new InvalidOperationException("Data source connection information is missing 'url'.");

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
