using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Elastic;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.FolderCollection.Config;
using TNO.Services.Managers;

namespace TNO.Services.FolderCollection;

/// <summary>
/// FolderCollectionManager class, provides a Kafka Listener service which collects content and saves it to folders.
/// </summary>
public class FolderCollectionManager : ServiceManager<FolderCollectionOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka admin client.
    /// </summary>
    protected IKafkaAdmin KafkaAdmin { get; private set; }

    /// <summary>
    /// get - Kafka message consumer.
    /// </summary>
    protected IKafkaListener<string, IndexRequestModel> Listener { get; private set; }

    /// <summary>
    /// get - The Elasticsearch configuration options.
    /// </summary>
    protected ElasticOptions ElasticOptions { get; private set; }

    /// <summary>
    /// get - The Elasticsearch client.
    /// </summary>
    protected ITNOElasticClient Client { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FolderCollectionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="elasticClient"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="kafkaAdmin"></param>
    /// <param name="consumer"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FolderCollectionManager(
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
        IKafkaAdmin kafkaAdmin,
        IKafkaListener<string, IndexRequestModel> consumer,
        IApiService api,
        IOptions<FolderCollectionOptions> options,
        ILogger<FolderCollectionManager> logger)
        : base(api, options, logger)
    {
        this.Client = elasticClient;
        this.ElasticOptions = elasticOptions.Value;
        this.KafkaAdmin = kafkaAdmin;
        this.Listener = consumer;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Continually polls for updated configuration.
    /// Listens to Kafka topic(s) for content to add to folders.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var delay = this.Options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
            {
                // An API request or failures have requested the service to stop.
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();

                // The service is stopping or has stopped, consume should stop too.
                this.Listener.Stop();
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    // Only include topics that exist.
                    var topics = this.Options.GetTopics();
                    var kafkaTopics = this.KafkaAdmin.ListTopics();
                    topics = topics.Except(topics.Except(kafkaTopics)).ToArray();

                    if (topics.Length > 0)
                    {
                        this.Listener.Subscribe(topics);
                        ConsumeMessages();
                    }
                    else if (topics.Length == 0)
                    {
                        this.Listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Service had an unexpected failure.");
                    this.State.RecordFailure();
                }
            }

            // The delay ensures we don't have a run-away thread.
            // With a minimum delay for all data source schedules, it could mean some data sources are pinged more often then required.
            // It could also result in a longer than planned delay if the action manager is awaited (currently it is).
            this.Logger.LogDebug("Service sleeping for {delay:n0} ms", delay);
            // await Thread.Sleep(new TimeSpan(0, 0, 0, delay));
            await Task.Delay(delay);
        }
    }

    /// <summary>
    /// Creates a new cancellation token.
    /// Create a new thread if the prior one isn't running anymore.
    /// </summary>
    private void ConsumeMessages()
    {
        if (_consumer == null || _notRunning.Contains(_consumer.Status))
        {
            // Make sure the prior task is cancelled before creating a new one.
            if (_cancelToken?.IsCancellationRequested == false)
                _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();
            _consumer = Task.Run(ListenerHandlerAsync, _cancelToken.Token);
        }
    }

    /// <summary>
    /// Keep consuming messages from Kafka until the service stops running.
    /// </summary>
    /// <returns></returns>
    private async Task ListenerHandlerAsync()
    {
        while (this.State.Status == ServiceStatus.Running &&
            _cancelToken?.IsCancellationRequested == false)
        {
            await this.Listener.ConsumeAsync(HandleMessageAsync, _cancelToken.Token);
        }

        // The service is stopping or has stopped, consume should stop too.
        this.Listener.Stop();
    }

    /// <summary>
    /// The Kafka consumer has failed for some reason, need to record the failure.
    /// Fatal or unexpected errors will result in a request to stop consuming.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns>True if the consumer should retry the message.</returns>
    private void ListenerErrorHandler(object sender, ErrorEventArgs e)
    {
        // Only the first retry will count as a failure.
        if (_retries == 0)
            this.State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            if (consume.Error.IsFatal)
                this.Listener.Stop();
        }
    }

    /// <summary>
    /// The Kafka consumer has stopped which means we need to also cancel the background task associated with it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListenerStopHandler(object sender, EventArgs e)
    {
        if (_consumer != null &&
            !_notRunning.Contains(_consumer.Status) &&
            _cancelToken != null && !_cancelToken.IsCancellationRequested)
        {
            _cancelToken.Cancel();
        }
    }

    /// <summary>
    /// Import the content.
    /// Copy any file associated with source content.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task HandleMessageAsync(ConsumeResult<string, IndexRequestModel> result)
    {
        try
        {
            this.Logger.LogInformation("FolderCollection content from Topic: {Topic}, Content ID: {Key}", result.Topic, result.Message.Key);
            var model = result.Message.Value;

            if (model.Action == IndexAction.Delete)
            {
                await this.Api.RemoveContentFromFoldersAsync(model.ContentId);
            }
            else
            {
                // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
                await ProcessRequestAsync(model);
            }

            // Successful run clears any errors.
            this.State.ResetFailures();
            _retries = 0;
        }
        catch (HttpClientRequestException ex)
        {
            this.Logger.LogError(ex, "HTTP exception while consuming. {response}", ex.Data["body"] ?? "");
        }
    }

    /// <summary>
    /// Process the request and determine if the content should be added to a folder.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task ProcessRequestAsync(IndexRequestModel request)
    {
        var content = await this.Api.FindContentByIdAsync(request.ContentId);
        if (content != null)
        {
            // TODO: Review how we can cache filters so that we do not need to request them every time we index content.
            var folders = await this.Api.GetFoldersWithFiltersAsync() ?? Array.Empty<API.Areas.Services.Models.Folder.FolderModel>();

            if (folders.Any())
            {
                this.Logger.LogInformation("Content being processed by folder filters.  Content ID: {contentId}", content.Id);
            }

            // Check if content should be added to each folder.
            foreach (var folder in folders.Where(f => f.Filter != null))
            {
                await ProcessFolderAsync(request, content, folder);
            }
        }
        else
        {
            this.Logger.LogWarning("Content does not exists. Content ID: {ContentId}", request.ContentId);
        }
    }

    /// <summary>
    /// Determine if content should be added or removed from a folder.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="content"></param>
    /// <param name="folder"></param>
    /// <returns></returns>
    private async Task ProcessFolderAsync(IndexRequestModel request, API.Areas.Services.Models.Content.ContentModel content, API.Areas.Services.Models.Folder.FolderModel folder)
    {
        if (folder.FilterId.HasValue && folder.Filter == null)
        {
            this.Logger.LogError("The folder '{folderId}:{name}' is missing a filter '{filterId}'.", folder.Id, folder.Name, folder.FilterId);
            return;
        }

        // Ignore any folders without filters.
        if (folder.Filter == null) return;

        if (request.Action == IndexAction.Unpublish && folder.Filter.Settings.SearchUnpublished == false)
        {
            this.Logger.LogDebug("Content being removed from folder.  Content ID: {contentId}, Folder ID: {folderId}", content.Id, folder.Id);
            // Remove unpublished content from folders that filter out unpublished content.
            await this.Api.RemoveContentFromFoldersAsync(content.Id);
        }
        else if (await RunFilterAsync(content, folder.Filter))
        {
            this.Logger.LogDebug("Content being added to folder.  Content ID: {contentId}, Folder ID: {folderId}", content.Id, folder.Id);
            // TODO: Sort order of content added to a folder should be configurable.
            await this.Api.AddContentToFolderAsync(content.Id, folder.Id);
        }
    }

    /// <summary>
    /// Run the filter against the content to determine if it should be added to the folder.
    /// Manually compare the content to the filter to reduce the need to make a request to Elasticsearch for easy comparisons.
    /// Make a request to Elasticsearch for keyword searches.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="filter"></param>
    /// <returns>Whether the content is returned by the filter.</returns>
    private async Task<bool> RunFilterAsync(API.Areas.Services.Models.Content.ContentModel content, API.Areas.Services.Models.Folder.FilterModel filter)
    {
        // Ignore empty Elasticsearch queries.
        if (IsEmpty(filter.Query)) return false;

        var now = DateTime.Now.ToLocalTime();

        if (!filter.Settings.SearchUnpublished && content.Status == Entities.ContentStatus.Draft) return false;
        if (filter.Settings.ContentTypes.Any() && !filter.Settings.ContentTypes.Contains(content.ContentType)) return false;
        if (filter.Settings.SourceIds.Any() && content.SourceId.HasValue && !filter.Settings.SourceIds.Contains(content.SourceId.Value)) return false;
        if (filter.Settings.ProductIds.Any() && !filter.Settings.ProductIds.Contains(content.ProductId)) return false;
        if (filter.Settings.ContributorIds.Any() && content.ContributorId.HasValue && !filter.Settings.ContributorIds.Contains(content.ContributorId.Value)) return false;
        if (filter.Settings.SeriesIds.Any() && content.SeriesId.HasValue && !filter.Settings.SeriesIds.Contains(content.SeriesId.Value)) return false;
        if (filter.Settings.Tags.Any() && !filter.Settings.Tags.Any(st => content.Tags.Any(t => t.Code.Equals(st, StringComparison.OrdinalIgnoreCase)))) return false;
        if (filter.Settings.Sentiment.Any())
        {
            var min = filter.Settings.Sentiment.Min();
            var max = filter.Settings.Sentiment.Max();
            // TODO: Need to handle custom tone pools.
            if (!content.TonePools.Any(tp => tp.Value >= min && tp.Value <= max)) return false;
        };
        if (filter.Settings.Actions.Any() && !filter.Settings.Actions.Any(fa => content.Actions.Any(a => a.Id == fa.Id && a.Value == fa.Value))) return false;

        var publishedOn = content.PublishedOn?.ToLocalTime();

        if (filter.Settings.StartDate != null && publishedOn < filter.Settings.StartDate) return false;
        if (filter.Settings.EndDate != null && publishedOn > filter.Settings.EndDate) return false;
        if (filter.Settings.DateOffset.HasValue && publishedOn > now.AddDays(filter.Settings.DateOffset.Value)) return false;

        if (filter.Settings.Edition != null && !content.Edition.Equals(filter.Settings.Edition, StringComparison.OrdinalIgnoreCase)) return false;
        if (filter.Settings.Section != null && !content.Section.Equals(filter.Settings.Section, StringComparison.OrdinalIgnoreCase)) return false;
        if (filter.Settings.Page != null && !content.Page.Equals(filter.Settings.Page, StringComparison.OrdinalIgnoreCase)) return false;

        if (filter.Settings.Search != null)
        {
            var index = filter.Settings.SearchUnpublished ? this.ElasticOptions.UnpublishedIndex : this.ElasticOptions.PublishedIndex;
            var query = ModifyElasticQuery(filter.Query, content.Id);
            var result = await this.Client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(index, query);

            // If the content item wasn't returned it wasn't a match.
            if (result.Hits.Total.Value != 1) return false;
        }

        // We passed all filter values so this content should be added to the folder.
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Check if the filter query is an empty query.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    private static bool IsEmpty(JsonDocument query)
    {
        var json = query.ToJson();
        if (json == "{}") return true;
        var node = JsonNode.Parse(json)?.AsObject();
        if (node == null) return true;
        return node.ContainsKey("query");
    }

    /// <summary>
    /// Modify the Elasticsearch query to include the content ID.
    /// This will ensure we only compare a single content item in the search and reduce the bandwidth needed.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="contentId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private JsonDocument ModifyElasticQuery(JsonDocument query, long contentId)
    {
        var json = JsonNode.Parse(query.ToJson())?.AsObject();
        if (json == null) return query;

        var jMustTerm = JsonNode.Parse($"{{ \"term\": {{ \"id\": {contentId} }}}}")?.AsObject() ?? throw new InvalidOperationException("Failed to parse JSON");
        if (json.TryGetPropertyValue("query", out JsonNode? jQuery))
        {
            if (jQuery?.AsObject().TryGetPropertyValue("bool", out JsonNode? jQueryBool) == true)
            {
                if (jQueryBool?.AsObject().TryGetPropertyValue("must", out JsonNode? jQueryBoolMust) == true)
                {
                    jQueryBoolMust?.AsArray().Add(jMustTerm);
                }
                else
                {
                    jQueryBool?.AsObject().Add("must", JsonNode.Parse($"[ {jMustTerm.ToJsonString()} ]"));
                }
            }
            else
            {
                jQuery?.AsObject().Add("bool", JsonNode.Parse($"{{ \"must\": [ {jMustTerm.ToJsonString()} ]}}"));
            }
        }
        else
        {
            json.Add("query", JsonNode.Parse($"{{ \"bool\": {{ \"must\": [ {jMustTerm.ToJsonString()} ] }}}}"));
        }
        return JsonDocument.Parse(json.ToJsonString());
    }
    #endregion
}
