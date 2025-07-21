using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Entities;
using TNO.Models.Filters;
using TNO.Services;
using TNO.Services.Managers;
using TNO.Tools.ElasticIndexer.Config;

namespace TNO.Tools.ElasticIndexer;

public class IndexerManager : ServiceManager<IndexerOptions>
{
    #region Variables
    private readonly IContentService _contentService;
    private readonly JsonSerializerOptions _serializationOptions;
    #endregion

    #region Properties
    /// <summary>
    /// get - The Elasticsearch client.
    /// </summary>
    protected ElasticsearchClient Client { get; private set; }

    /// <summary>
    /// get/set - Elasticsearch configuration settings.
    /// </summary>
    protected ElasticOptions ElasticOptions { get; set; }
    #endregion

    #region Constructors
    public IndexerManager(
        IContentService contentService,
        IApiService api,
        IChesService chesService,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<JsonSerializerOptions> serializationOptions,
        IOptions<ChesOptions> chesOptions,
        IOptions<IndexerOptions> options,
        ILogger<ServiceManager<IndexerOptions>> logger)
        : base(api, chesService, chesOptions, options, logger)
    {
        _contentService = contentService;
        _serializationOptions = serializationOptions.Value;
        this.ElasticOptions = elasticOptions.Value;
        var connect = new ElasticsearchClientSettings(this.ElasticOptions.Url);
        if (!String.IsNullOrWhiteSpace(this.ElasticOptions.Username) && !String.IsNullOrWhiteSpace(this.ElasticOptions.Password))
            connect.Authentication(new BasicAuthentication(this.ElasticOptions.Username, this.ElasticOptions.Password));
        else if (!String.IsNullOrWhiteSpace(this.ElasticOptions.ApiKey))
            connect.Authentication(new ApiKey(this.ElasticOptions.ApiKey));
        this.Client = new ElasticsearchClient(connect);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Keep running until a critical error occurs.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var keepRunning = true;
        var delay = this.Options.DefaultDelayMS;

        while (keepRunning)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
            {
                // An API request or failures have requested the service to stop.
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    keepRunning = await IndexContentAsync();
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Service had an unexpected failure.");
                    this.State.RecordFailure();
                    await this.SendErrorEmailAsync("Service had an Unexpected Failure", ex);
                }
            }

            // The delay ensures we don't have a run-away thread.
            // With a minimum delay for all data source schedules, it could mean some data sources are pinged more often then required.
            // It could also result in a longer than planned delay if the action manager is awaited (currently it is).
            this.Logger.LogDebug("Service sleeping for {delay:n0} ms", delay);
            await Task.Delay(delay);
        }
    }

    /// <summary>
    /// Keep on asking for content from the database and sending it to Elasticsearch.
    /// Stop when there are too many sequential failures.
    /// </summary>
    /// <returns></returns>
    private async Task<bool> IndexContentAsync()
    {
        TNO.Entities.Models.IPaged<TNO.Entities.Content> results;
        var count = 0;
        do
        {
            var success = true;
            var contentFilter = new ContentFilter()
            {
                Page = this.Options.Page,
                Quantity = this.Options.Quantity,
                CreatedStartOn = this.Options.CreatedStartOn,
                CreatedEndOn = this.Options.CreatedEndOn,
                SourceIds = this.Options.SourceIds,
                Sort = ["id"],
            };
            // Make a request for more content.
            results = _contentService.FindWithDatabase(contentFilter);
            count += results.Items.Count;

            var bulkRequestAllContent = new BulkRequest(this.ElasticOptions.ContentIndex)
            {
                Operations = new List<IBulkOperation>(),
            };
            var bulkRequestPublishedContent = new BulkRequest(this.ElasticOptions.PublishedIndex)
            {
                Operations = new List<IBulkOperation>(),
            };

            foreach (var content in results.Items)
            {
                var contentModel = new ContentModel(content, _serializationOptions);
                if (!content.IsApproved && content.ContentType == ContentType.AudioVideo) content.Body = "";

                if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                {
                    contentModel.Status = ContentStatus.Published;
                    var indexPublishedOperation = new BulkIndexOperation<ContentModel>(contentModel)
                    {
                        Id = content.Id,
                    };
                    bulkRequestPublishedContent.Operations.Add(indexPublishedOperation);
                }

                var indexOperation = new BulkIndexOperation<ContentModel>(contentModel)
                {
                    Id = content.Id,
                };
                bulkRequestAllContent.Operations.Add(indexOperation);
            }

            // Index all content to cloud
            if (bulkRequestAllContent.Operations.Count > 0)
            {
                var allResponse = await this.Client.BulkAsync(bulkRequestAllContent);
                if (!allResponse.IsValidResponse)
                {
                    success = false;
                    this.State.RecordFailure();
                    this.Logger.LogError("Failed to index all content.  Error: {error}", allResponse.DebugInformation);
                    foreach (var item in allResponse.ItemsWithErrors)
                    {
                        this.Logger.LogError("Error for document: {id}: {error}", item.Id, item.Error?.Reason);
                    }
                }
            }

            if (bulkRequestPublishedContent.Operations.Count > 0)
            {
                var publishedResponse = await this.Client.BulkAsync(bulkRequestPublishedContent);
                if (!publishedResponse.IsValidResponse)
                {
                    success = false;
                    this.State.RecordFailure();
                    this.Logger.LogError("Failed to index published content.  Error: {error}", publishedResponse.DebugInformation);
                    foreach (var item in publishedResponse.ItemsWithErrors)
                    {
                        this.Logger.LogError("Error for document: {id}: {error}", item.Id, item.Error?.Reason);
                    }
                }
            }

            if (success)
            {
                this.State.ResetFailures();
                this.Logger.LogInformation("Indexed page: {page}, total items: {count:N0}", this.Options.Page, count);
                this.Options.Page++;
            }
            else
            {
                this.Logger.LogError("Failed to index.  Indexed page: {page}, total items: {count:N0}", this.Options.Page, count);
            }

            if (this.Options.DefaultDelayMS > 0)
                await Task.Delay(this.Options.DefaultDelayMS);

            // Run until failure, or no more content, or configured limit
        } while (
            this.State.Status == ServiceStatus.Running
            && results.Items.Count == this.Options.Quantity
            && (!this.Options.MaxPage.HasValue || this.Options.Page <= this.Options.MaxPage));

        // This will stop the main processing loop.
        return false;
    }
    #endregion
}
