using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Managers;
using TNO.Services.NLP.Config;
using TNO.Services.NLP.OpenNLP;

namespace TNO.Services.NLP;

/// <summary>
/// NlpManager class, provides a common way to manager the actions this service performs.
/// Each time the manager is run it will execute NLP processes to extract data.
/// </summary>
public class NlpManager : ServiceManager<NLPOptions>
{
    #region Variables
    private readonly EntityExtractor _extractor = new();
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private readonly WorkOrderStatus[] _ignoreWorkOrders = new WorkOrderStatus[] { WorkOrderStatus.Completed, WorkOrderStatus.Cancelled, WorkOrderStatus.Failed };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Listener object.
    /// </summary>
    protected IKafkaListener<string, NlpRequestModel> Listener { get; private set; }

    /// <summary>
    /// get - Kafka message producer.
    /// </summary>
    protected IKafkaMessenger Producer { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NlpManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="producer"></param>
    /// <param name="api"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public NlpManager(
        IKafkaListener<string, NlpRequestModel> consumer,
        IKafkaMessenger producer,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<NLPOptions> options,
        ILogger<NlpManager> logger)
        : base(api, chesService, chesOptions, options, logger)
    {
        this.Producer = producer;
        this.Listener = consumer;
        this.Listener.IsLongRunningJob = true;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Continues to update the Kafka consumer if the configuration changes.
    /// </summary>
    /// <returns></returns>
    public async override Task RunAsync()
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
                }
            }

            // The delay ensures we don't have a run away thread.
            this.Logger.LogDebug("Service sleeping for {delay} ms", delay);
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
    /// Retrieve a file from storage and send to Microsoft Cognitive Services. Obtain
    /// the transcription and update the content record accordingly.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(ConsumeResult<string, NlpRequestModel> result)
    {
        try
        {
            var request = result.Message.Value;
            // The service has stopped, so to should consuming messages.
            if (this.State.Status != ServiceStatus.Running)
            {
                this.Listener.Stop();
                this.State.Stop();
            }
            else
            {
                var content = await this.Api.FindContentByIdAsync(request.ContentId);
                if (content != null)
                {
                    // If the content was published before the specified offset, ignore it.
                    if (Options.IgnoreContentPublishedBeforeOffset.HasValue
                        && Options.IgnoreContentPublishedBeforeOffset > 0
                        && content.PublishedOn.HasValue
                        && content.PublishedOn.Value < DateTime.UtcNow.AddDays(-1 * Options.IgnoreContentPublishedBeforeOffset.Value))
                        return;

                    await UpdateContentAsync(request);
                    await SendIndexingRequest(content);
                }
                else
                {
                    // Identify requests for transcription for content that does not exist.
                    this.Logger.LogWarning("Content does not exist for this message. Key: {Key}, Content ID: {ContentId}", result.Message.Key, request.ContentId);
                }

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
            }
            else
            {
                this.Logger.LogError(ex, "Failed to handle message");
            }
            ListenerErrorHandler(this, new ErrorEventArgs(ex));
        }
    }

    /// <summary>
    /// Make a request to parse text with natural language processing.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task UpdateContentAsync(NlpRequestModel request)
    {
        this.Logger.LogInformation("NLP requested.  Content ID: {Id}", request.ContentId);
        var hasWorkOrder = await UpdateWorkOrderAsync(request, WorkOrderStatus.InProgress);

        if (hasWorkOrder)
        {
            // Fetch content again because it may have been updated by an external source.
            // This can introduce issues if the transcript has been edited as now it will overwrite what was changed.
            var content = await this.Api.FindContentByIdAsync(request.ContentId);

            if (content != null)
            {
                var labels = await ExtractEntitiesAsync(content); // TODO: Extract language from data source.
                if (labels.Any())
                {
                    // Only add new labels.
                    var originalLabels = new List<ContentLabelModel>(content.Labels);
                    foreach (var label in labels)
                    {
                        var original = content.Labels.FirstOrDefault(l => l.Key == label.Key && l.Value == label.Value);
                        if (original == null) originalLabels.Add(label);
                    }
                    content.Labels = originalLabels.ToArray();

                    await this.Api.UpdateContentAsync(content); // TODO: This can result in an editor getting a optimistic concurrency error.
                    this.Logger.LogInformation("Labels updated.  Content ID: {Id}", request.ContentId);

                    await UpdateWorkOrderAsync(request, WorkOrderStatus.Completed);
                }
                else if (!labels.Any())
                {
                    this.Logger.LogWarning("Content did not generate a labels. Content ID: {Id}", request.ContentId);
                    await UpdateWorkOrderAsync(request, WorkOrderStatus.Completed);
                }
            }
            else
            {
                // The content is no longer available for some reason.
                this.Logger.LogError("Content no longer exists. Content ID: {Id}", request.ContentId);
                await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed);
            }
        }
        else
        {
            this.Logger.LogWarning("Request ignored because it does not have a work order");
        }
    }

    /// <summary>
    /// Update the work order (if it exists) with the specified 'status'.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="status"></param>
    /// <returns>Whether a work order exists or is not required.</returns>
    private async Task<bool> UpdateWorkOrderAsync(NlpRequestModel request, WorkOrderStatus status)
    {
        if (request.WorkOrderId > 0)
        {
            var workOrder = await this.Api.FindWorkOrderAsync(request.WorkOrderId);
            if (workOrder != null && !_ignoreWorkOrders.Contains(workOrder.Status))
            {
                workOrder.Status = status;
                await this.Api.UpdateWorkOrderAsync(workOrder);
                return true;
            }
        }
        return !this.Options.AcceptOnlyWorkOrders;
    }

    /// <summary>
    /// Make a request to OpenNLP models to extract information from content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private Task<ContentLabelModel[]> ExtractEntitiesAsync(ContentModel content)
    {
        var labels = new List<ContentLabelModel>();
        labels.AddRange(ExtractEntity(content, EntityType.Person));
        labels.AddRange(ExtractEntity(content, EntityType.Organization));
        labels.AddRange(ExtractEntity(content, EntityType.Location));
        labels.AddRange(ExtractEntity(content, EntityType.Date));
        labels.AddRange(ExtractEntity(content, EntityType.Time));
        labels.AddRange(ExtractEntity(content, EntityType.Money));

        return Task.FromResult(labels.ToArray());
    }

    /// <summary>
    /// Process the content and extract the specified entity type.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="entityType"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private IEnumerable<ContentLabelModel> ExtractEntity(ContentModel content, EntityType entityType)
    {
        var text = new StringBuilder();
        text.AppendLine(content.Summary);
        if (!String.IsNullOrWhiteSpace(content.Body)) text.AppendLine(content.Body);
        return _extractor.ExtractEntities(text.ToString(), entityType).Distinct().Select(l => new ContentLabelModel()
        {
            ContentId = content.Id,
            Key = Enum.GetName(entityType) ?? throw new InvalidOperationException("Entity type does not exist"),
            Value = l
        });
    }

    /// <summary>
    /// Send a request to Kafka to update the indexes in Elasticsearch.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task SendIndexingRequest(ContentModel content)
    {
        if (!String.IsNullOrWhiteSpace(this.Options.IndexingTopic))
        {
            var result = await this.Producer.SendMessageAsync(this.Options.IndexingTopic, new IndexRequestModel(content, IndexAction.Index));
            if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka when submitting an indexing request.  ContentId: {content.Id}");

            if (content.Status == ContentStatus.Published)
            {
                result = await this.Producer.SendMessageAsync(this.Options.IndexingTopic, new IndexRequestModel(content, IndexAction.Publish));
                if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka when submitting an indexing request.  ContentId: {content.Id}");
            }
        }
    }
    #endregion
}
