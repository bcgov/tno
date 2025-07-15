using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Elastic;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Indexing.Config;
using TNO.Services.Managers;

namespace TNO.Services.Indexing;

/// <summary>
/// IndexingManager class, provides a Kafka Listener service which sends content to Elasticsearch for indexing.
/// </summary>
public class IndexingManager : ServiceManager<IndexingOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get/set - The elastic configuration options.
    /// </summary>
    protected ElasticOptions ElasticOptions { get; set; }

    /// <summary>
    /// get - Kafka admin client.
    /// </summary>
    protected IKafkaAdmin KafkaAdmin { get; private set; }

    /// <summary>
    /// get - Kafka message consumer.
    /// </summary>
    protected IKafkaListener<string, IndexRequestModel> Listener { get; private set; }

    /// <summary>
    /// get - The Elasticsearch client.
    /// </summary>
    protected ElasticsearchClient Client { get; private set; }

    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "NotificationContent_";
    private const int CacheExpirationSeconds = 5;

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IndexingManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafkaAdmin"></param>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="chesService"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="memoryCache"></param>
    public IndexingManager(
        IKafkaAdmin kafkaAdmin,
        IKafkaListener<string, IndexRequestModel> listener,
        IApiService api,
        IChesService chesService,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<ChesOptions> chesOptions,
        IOptions<IndexingOptions> options,
        ILogger<IndexingManager> logger,
        IMemoryCache memoryCache)
        : base(api, chesService, chesOptions, options, logger)
    {
        this.ElasticOptions = elasticOptions.Value;
        this.KafkaAdmin = kafkaAdmin;
        this.Listener = listener;
        this.Listener.IsLongRunningJob = false;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;

        // TODO: Change to dependency injection.
        var connect = new ElasticsearchClientSettings(this.ElasticOptions.Url);

        if (!String.IsNullOrWhiteSpace(this.ElasticOptions.Username) && !String.IsNullOrWhiteSpace(this.ElasticOptions.Password))
            connect.Authentication(new BasicAuthentication(this.ElasticOptions.Username, this.ElasticOptions.Password));
        else if (!String.IsNullOrWhiteSpace(this.ElasticOptions.ApiKey))
            connect.Authentication(new ApiKey(this.ElasticOptions.ApiKey));
        this.Client = new ElasticsearchClient(connect);

        _cache = memoryCache;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Continually polls for updated configuration.
    /// Listens to Kafka topic(s) for content to send to Elasticsearch.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var delay = this.Options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
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
                    var topics = this.Options.Topics.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    // Only include topics that exist.
                    var kafkaTopics = this.KafkaAdmin.ListTopics();
                    topics = topics.Except(topics.Except(kafkaTopics)).ToArray();

                    if (topics.Length != 0)
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
    /// Creates a new cancellation token.
    /// Create a new thread if the prior one isn't running anymore.
    /// </summary>
    private void ConsumeMessages()
    {
        if (_consumer == null || _notRunning.Contains(_consumer.Status))
        {
            this.Logger.LogDebug("ConsumeMessages:_consumer is null");
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
            this.Logger.LogDebug("ListenerHandlerAsync:AWAIT");
            await this.Listener.ConsumeAsync(HandleMessageAsync, _cancelToken.Token);
        }

        // The service is stopping or has stopped, consume should stop too.
        this.Logger.LogDebug("ListenerHandlerAsync:STOP");
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
        this.Logger.LogDebug("ListenerErrorHandler: Retries={_retries}", _retries);
        // Only the first retry will count as a failure.
        if (_retries == 0)
            this.State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            if (consume.Error.IsFatal)
            {
                this.Listener.Stop();
                this.Logger.LogDebug("ListenerErrorHandler: Stop on IsFatal");
            }
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
            this.Logger.LogDebug("ListenerStopHandler.Cancel");
            _cancelToken.Cancel();
        }
    }

    /// <summary>
    /// Add/Update/remove the content index.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task HandleMessageAsync(ConsumeResult<string, IndexRequestModel> result)
    {
        try
        {
            // The service has stopped, so to should consuming messages.
            if (this.State.Status != ServiceStatus.Running)
            {
                this.Listener.Stop();
                this.State.Stop();
            }
            else
            {
                await ProcessIndexRequestAsync(result);

                // Inform Kafka this message is completed.
                this.Listener.Commit(result);
                this.Listener.Resume();

                // Successful run clears any errors.
                this.State.ResetFailures();
                _retries = 0;
            }
        }
        catch (Exception ex)
        {
            if (ex is HttpClientRequestException httpEx)
            {
                this.Logger.LogError(ex, "HTTP exception while consuming. {response}", httpEx.Data["body"] ?? "");
                await this.SendErrorEmailAsync("HTTP exception while consuming. {response}", ex);
            }
            else
            {
                this.Logger.LogError(ex, "Failed to handle message");
                await this.SendErrorEmailAsync("Failed to handle message", ex);
            }
            ListenerErrorHandler(this, new ErrorEventArgs(ex));
        }
        finally
        {
            if (State.Status == ServiceStatus.Running) Listener.Resume();
        }
    }

    /// <summary>
    /// Process the index update request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessIndexRequestAsync(ConsumeResult<string, IndexRequestModel> result)
    {
        this.Logger.LogInformation("Indexing content from Topic: {Topic}, Content ID: {Key}", result.Topic, result.Message.Key);
        var model = result.Message.Value;

        if (model.Action == IndexAction.Delete)
        {
            await DeleteContentAsync(model.ContentId);
        }
        else
        {
            // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
            var content = await this.Api.FindContentByIdAsync(result.Message.Value.ContentId);
            if (content != null)
            {
                if (model.Action == IndexAction.Publish)
                    content = await PublishContentAsync(content);
                else if (model.Action == IndexAction.Unpublish)
                    content = await UnpublishContentAsync(content);

                // Update the unpublished content with the latest data and status.
                await IndexContentAsync(result.Message.Value, content);
            }
            else
            {
                this.Logger.LogWarning("Content does not exists. Content ID: {ContentId}", result.Message.Value.ContentId);
            }
        }
        // Indexing is completed, pass the baton to the folder process.
        await this.Api.SendMessageAsync(model);
    }

    /// <summary>
    /// Send content to Elasticsearch unpublished index.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task IndexContentAsync(IndexRequestModel request, ContentModel content)
    {
        var document = new IndexRequest<ContentModel>(content, this.ElasticOptions.ContentIndex, content.Id);
        var response = await this.Client.IndexAsync(document);
        if (response.IsSuccess())
        {
            this.Logger.LogInformation("Content indexed.  Content ID: {id}, Index: {index}, Version: {version}", content.Id, this.ElasticOptions.ContentIndex, content.Version);

            // Tell the API to inform users of published content.
            if (!this.Options.IndexOnly)
                await SendNotifications(request, content);
        }
        else
        {
            // TODO: Need to find a way to inform the Editor it failed.  Send notification message to them.
            if (response.TryGetOriginalException(out Exception? ex))
                this.Logger.LogError(ex, "Content failed to index.  Content ID: {id}, Index: {index}", content.Id, this.ElasticOptions.ContentIndex);
        }
    }

    /// <summary>
    /// Having two copies of content regrettably can result in synchronization issues.
    /// Update status of content to 'Published'.
    /// Send update to database.
    /// Send content to published index.
    /// Send notifications.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task<ContentModel> PublishContentAsync(ContentModel content)
    {
        // Update the status of the content to indicate it has been published.
        if (content.Status != ContentStatus.Published)
        {
            content.Status = ContentStatus.Published;
            if (!this.Options.IndexOnly)
                content = await this.Api.UpdateContentStatusAsync(content) ?? throw new InvalidOperationException($"Content failed to update. ID:{content.Id}");
        }

        // Remove the transcript body if it hasn't been approved.
        var body = content.Body;
        if (!content.IsApproved && content.ContentType == ContentType.AudioVideo) content.Body = "";
        var document = new IndexRequest<ContentModel>(content, this.ElasticOptions.PublishedIndex, content.Id);
        var response = await this.Client.IndexAsync(document);
        content.Body = body;
        if (response.IsSuccess())
        {
            this.Logger.LogInformation("Content published.  Content ID: {id}, Index: {index}, Version: {version}", content.Id, this.ElasticOptions.PublishedIndex, content.Version);
        }
        else
        {
            // TODO: Need to find a way to inform the Editor it failed.  Send notification message to them.
            if (response.TryGetOriginalException(out Exception? ex))
                this.Logger.LogError(ex, "Content failed to publish.  Content ID: {id}, Index: {index}", content.Id, this.ElasticOptions.PublishedIndex);
        }

        return content;
    }

    /// <summary>
    /// Remove content from published index.
    /// Ensure content status is updated to reflect being published.
    /// Send notifications.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task<ContentModel> UnpublishContentAsync(ContentModel content)
    {
        // Update the status of the content to indicate it has been unpublished.
        if (content.Status != ContentStatus.Unpublished)
        {
            content.Status = ContentStatus.Unpublished;
            if (!this.Options.IndexOnly)
                content = await this.Api.UpdateContentStatusAsync(content) ?? throw new InvalidOperationException($"Content failed to update. ID:{content.Id}");
        }

        var document = new DeleteRequest<ContentModel>(content, this.ElasticOptions.PublishedIndex, content.Id);
        var response = await this.Client.DeleteAsync(document);
        if (response.IsSuccess())
        {
            this.Logger.LogInformation("Content unpublished.  Content ID: {id}, Index: {index}, Version: {version}", content.Id, this.ElasticOptions.PublishedIndex, content.Version);
        }
        else
        {
            // TODO: Need to find a way to inform the Editor it failed.  Send notification message to them.
            if (response.TryGetOriginalException(out Exception? ex))
                this.Logger.LogError(ex, "Content failed to unpublish.  Content ID: {id}, Index: {index}", content.Id, this.ElasticOptions.PublishedIndex);
        }
        return content;
    }

    /// <summary>
    /// Remove content from both indexes.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    private async Task DeleteContentAsync(long contentId)
    {
        await DeleteContentAsync(contentId, this.ElasticOptions.PublishedIndex);
        await DeleteContentAsync(contentId, this.ElasticOptions.ContentIndex);
    }

    /// <summary>
    /// Remove the content from the specified index.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private async Task DeleteContentAsync(long contentId, string index)
    {
        var request = new DeleteRequest<ContentModel>(index, contentId);
        var response = await this.Client.DeleteAsync(request);
        if (response.IsSuccess())
        {
            this.Logger.LogInformation("Content deleted.  Content ID: {id}, Index: {index}", contentId, index);
        }
        else
        {
            // TODO: Need to find a way to inform the Editor it failed.  Send notification message to them.
            if (response.TryGetOriginalException(out Exception? ex))
                this.Logger.LogError(ex, "Content failed to delete.  Content ID: {id}, Index: {index}", contentId, index);
        }
    }

    /// <summary>
    /// Make a request to the API to determine if there are any notifications that should be sent for the content.
    /// There could be transcript requests, alerts based on subscriber filters, or other types.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task SendNotifications(IndexRequestModel request, ContentModel content)
    {
        var cacheKey = $"{CacheKeyPrefix}{content.Id}";

        if (!_cache.TryGetValue(cacheKey, out _))
        {
            if (!String.IsNullOrWhiteSpace(this.Options.NotificationTopic))
            {
                // TODO: Make request to API to determine what notifications should be sent.
                // TODO: Generate appropriate notification request.
                var notification = new NotificationRequestModel(NotificationDestination.SignalR | NotificationDestination.NotificationService, content.Id)
                {
                    RequestorId = request.RequestorId,
                };
                _ = await this.Api.SendMessageAsync(notification)
                    ?? throw new HttpClientRequestException($"Failed to receive result from Kafka when sending message.  Topic: {this.Options.NotificationTopic}, Content ID: {content.Id}");

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(CacheExpirationSeconds))
                    .SetSize(1);

                // add to cache
                _cache.Set(cacheKey, true, cacheEntryOptions);
                this.Logger.LogInformation("Notification for Content {contentId} was sent, added to cache.", content.Id);
            }
        }
        else
        {
            this.Logger.LogInformation("Notification for Content {contentId} was recently sent, skipping this notification request.", content.Id);
        }
    }
    #endregion
}
