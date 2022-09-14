using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Entities;
using TNO.Kafka;
using TNO.Services.Managers;
using TNO.Services.Indexing.Config;
using TNO.API.Areas.Services.Models.Content;
using TNO.Kafka.Models;
using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using TNO.Core.Exceptions;

namespace TNO.Services.Indexing;

/// <summary>
/// IndexingManager class, provides a Kafka Consumer service which sends content to Elasticsearch for indexing.
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
    /// get - Kafka admin client.
    /// </summary>
    protected IKafkaAdmin KafkaAdmin { get; private set; }

    /// <summary>
    /// get - Kafka message consumer.
    /// </summary>
    protected IKafkaListener<string, IndexRequest> Consumer { get; private set; }

    /// <summary>
    /// get - Kafka message producer.
    /// </summary>
    protected IKafkaMessenger Producer { get; private set; }

    /// <summary>
    /// get - The Elasticsearch client.
    /// </summary>
    protected ElasticsearchClient Client { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IndexingManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafkaAdmin"></param>
    /// <param name="consumer"></param>
    /// <param name="producer"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public IndexingManager(
        IKafkaAdmin kafkaAdmin,
        IKafkaListener<string, IndexRequest> consumer,
        IKafkaMessenger producer,
        IApiService api,
        IOptions<IndexingOptions> options,
        ILogger<IndexingManager> logger)
        : base(api, options, logger)
    {
        this.KafkaAdmin = kafkaAdmin;
        this.Producer = producer;
        this.Consumer = consumer;
        this.Consumer.OnError += ConsumerErrorHandler;
        this.Consumer.OnStop += ConsumerStopHandler;

        // TODO: Change to dependency injection.
        var connect = new ElasticsearchClientSettings(new Uri(options.Value.ElasticsearchUri))
            .Authentication(new BasicAuthentication(_options.ElasticsearchUsername, _options.ElasticsearchPassword));
        this.Client = new ElasticsearchClient(connect);
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
        var delay = _options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
            {
                // An API request or failures have requested the service to stop.
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();

                // The service is stopping or has stopped, consume should stop too.
                this.Consumer.Stop();
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
                    var topics = _options.GetTopics();
                    var kafkaTopics = this.KafkaAdmin.ListTopics();
                    topics = topics.Except(topics.Except(kafkaTopics)).ToArray();

                    if (topics.Length > 0)
                    {
                        this.Consumer.Subscribe(topics);
                        ConsumeMessages();
                    }
                    else if (topics.Length == 0)
                    {
                        this.Consumer.Stop();
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
            _cancelToken = new CancellationTokenSource();
            _consumer = Task.Factory.StartNew(() => ConsumerHandler(), _cancelToken.Token);
        }
    }

    /// <summary>
    /// Keep consuming messages from Kafka until the service stops running.
    /// </summary>
    /// <returns></returns>
    private async Task ConsumerHandler()
    {
        while (this.State.Status == ServiceStatus.Running &&
            _cancelToken?.IsCancellationRequested == false)
        {
            await this.Consumer.ConsumeAsync(HandleMessageAsync);
        }

        // The service is stopping or has stopped, consume should stop too.
        this.Consumer.Stop();
    }

    /// <summary>
    /// The Kafka consumer has failed for some reason, need to record the failure.
    /// Fatal or unexpected errors will result in a request to stop consuming.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns>True if the consumer should retry the message.</returns>
    private ConsumerAction ConsumerErrorHandler(object sender, ErrorEventArgs e)
    {
        // Only the first retry will count as a failure.
        if (_retries == 0)
            this.State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            return consume.Error.IsFatal ? ConsumerAction.Stop : ConsumerAction.Retry;
        }

        return _options.RetryLimit > _retries++ ? ConsumerAction.Retry : ConsumerAction.Stop;
    }

    /// <summary>
    /// The Kafka consumer has stopped which means we need to also cancel the background task associated with it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConsumerStopHandler(object sender, EventArgs e)
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
    private async Task<ConsumerAction> HandleMessageAsync(ConsumeResult<string, IndexRequest> result)
    {
        try
        {
            this.Logger.LogInformation("Importing content from Topic: {Topic}, Uid: {Key}", result.Topic, result.Message.Key);

            // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.

            var content = await _api.FindContentByIdAsync(result.Message.Value.ContentId);
            if (content != null)
            {
                switch (result.Message.Value.Action)
                {
                    case IndexAction.Publish:
                        await PublishContentAsync(content);
                        break;
                    case IndexAction.Unpublish:
                        await UnpublishContentAsync(content);
                        break;
                    case IndexAction.Delete:
                        await DeleteContentAsync(content);
                        break;
                    case IndexAction.Index:
                    default:
                        await IndexContentAsync(content);
                        break;
                }
            }
            else
            {
                this.Logger.LogWarning("Content does not exists. Content ID: {ContentId}", result.Message.Value.ContentId);
            }

            // Successful run clears any errors.
            this.State.ResetFailures();
            _retries = 0;
            return ConsumerAction.Proceed;
        }
        catch (HttpClientRequestException ex)
        {
            this.Logger.LogError(ex, "HTTP exception while consuming. {response}", ex.Data["body"] ?? "");
        }

        return _options.RetryLimit > _retries++ ? ConsumerAction.Retry : ConsumerAction.Stop;
    }

    /// <summary>
    /// Send content to Elasticsearch unpublished index.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task IndexContentAsync(ContentModel content)
    {
        var document = new IndexRequest<ContentModel>(content, _options.UnpublishedIndex, content.Uid);
        var response = await this.Client.IndexAsync(document);
        if (response.IsSuccess())
        {
            this.Logger.LogInformation("Content indexed.  Content ID: {Id}, Index: {PublishedIndex}", content?.Id, _options.PublishedIndex);
        }
        else
        {
            // TODO: Need to find a way to inform the Editor it failed.  Send notification message to them.
            this.Logger.LogError(response.OriginalException, "Content failed to index.  Content ID: {Id}, Index: {PublishedIndex}", content?.Id, _options.PublishedIndex);
        }
    }

    /// <summary>
    /// Send content to published index.
    /// Ensure content status is updated to reflect being published.
    /// Send notifications.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task PublishContentAsync(ContentModel content)
    {
        var document = new IndexRequest<ContentModel>(content, _options.PublishedIndex, content.Uid);
        var response = await this.Client.IndexAsync(document);
        if (response.IsSuccess())
        {
            // Update the status of the content to indicate it has been published.
            if (content.Status != ContentStatus.Published)
            {
                content.Status = ContentStatus.Published;
                await _api.UpdateContentAsync(content);
            }
            this.Logger.LogInformation("Content published.  Content ID: {Id}, Index: {PublishedIndex}", content?.Id, _options.PublishedIndex);

            await SendNotifications(content!);
        }
        else
        {
            // TODO: Need to find a way to inform the Editor it failed.  Send notification message to them.
            this.Logger.LogError(response.OriginalException, "Content failed to publish.  Content ID: {Id}, Index: {PublishedIndex}", content?.Id, _options.PublishedIndex);
        }
    }

    /// <summary>
    /// Remove content from published index.
    /// Ensure content status is updated to reflect being published.
    /// Send notifications.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task UnpublishContentAsync(ContentModel content)
    {
        var document = new DeleteRequest<ContentModel>(content, _options.PublishedIndex, content.Uid);
        var response = await this.Client.DeleteAsync(document);
        if (response.IsSuccess())
        {
            // Update the status of the content to indicate it has been unpublished.
            if (content.Status != ContentStatus.Unpublished)
            {
                content.Status = ContentStatus.Unpublished;
                await _api.UpdateContentAsync(content);
            }
            this.Logger.LogInformation("Content unpublished.  Content ID: {Id}, Index: {PublishedIndex}", content?.Id, _options.PublishedIndex);

            await SendNotifications(content!);
        }
        else
        {
            // TODO: Need to find a way to inform the Editor it failed.  Send notification message to them.
            this.Logger.LogError(response.OriginalException, "Content failed to publish.  Content ID: {Id}, Index: {PublishedIndex}", content?.Id, _options.PublishedIndex);
        }
    }

    /// <summary>
    /// Remove content from both indexes.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task DeleteContentAsync(ContentModel content)
    {
        if (content.Status == ContentStatus.Published)
        {
            await DeleteContentAsync(content, _options.PublishedIndex);
        }
        await DeleteContentAsync(content, _options.UnpublishedIndex);
    }

    /// <summary>
    /// Remove the content from the specified index.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private async Task DeleteContentAsync(ContentModel content, string index)
    {
        var document = new DeleteRequest<ContentModel>(content, index, content.Uid);
        var response = await this.Client.DeleteAsync(document);
        if (response.IsSuccess())
        {
            this.Logger.LogInformation("Content removed.  Content ID: {Id}, Index: {index}", content.Id, index);
        }
        else
        {
            // TODO: Need to find a way to inform the Editor it failed.  Send notification message to them.
            this.Logger.LogError(response.OriginalException, "Content failed to be removed.  Content ID: {Id}, Index: {index}", content.Id, index);
        }
    }

    /// <summary>
    /// Make a request to the API to determine if there are any notifications that should be sent for the content.
    /// There could be transcript requests, alerts based on subscriber filters, or other types.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task SendNotifications(ContentModel content)
    {
        if (!String.IsNullOrWhiteSpace(_options.NotificationTopic))
        {
            // TODO: Make request to API to determine what natifications should be sent.
            // TODO: Generate appropriate notification request.
            var notification = new NotificationRequest(0, content.Id, 0, 0);
            var result = await this.Producer.SendMessageAsync(_options.NotificationTopic, content.Uid, notification);
            if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka when submitting a notification request.  ContentId: {content.Id}");
        }
    }
    #endregion
}
