using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.Managers;
using TNO.Services.Content.Config;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;

namespace TNO.Services.Content;

/// <summary>
/// ContentManager class, provides a Kafka Consumer service which imports content from all active topics.
/// </summary>
public class ContentManager : ServiceManager<ContentOptions>
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
    protected IKafkaListener<string, SourceContent> Consumer { get; private set; }

    /// <summary>
    /// get - Kafka message producer.
    /// </summary>
    protected IKafkaMessenger Producer { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafkaAdmin"></param>
    /// <param name="kafkaListener"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentManager(
        IKafkaAdmin kafkaAdmin,
        IKafkaListener<string, SourceContent> kafkaListener,
        IKafkaMessenger kafkaMessenger,
        IApiService api,
        IOptions<ContentOptions> options,
        ILogger<ContentManager> logger)
        : base(api, options, logger)
    {
        this.KafkaAdmin = kafkaAdmin;
        this.Producer = kafkaMessenger;
        this.Consumer = kafkaListener;
        this.Consumer.OnError += ConsumerErrorHandler;
        this.Consumer.OnStop += ConsumerStopHandler;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Continually poll for updated data source configuration.
    /// When there are topics to listen too it will initialize a Kafka consumer.
    /// When configuration updates result in changes to topics, it will update which topics it is subscribing too.
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
                    // TODO: Handle e-tag.
                    var ingest = (await _api.GetIngestsAsync()).ToArray();

                    // Listen to every enabled data source with a topic that is configured to produce content.
                    var topics = _options.GetContentTopics(ingest
                        .Where(i => i.IsEnabled &&
                            !String.IsNullOrWhiteSpace(i.Topic) &&
                            i.ImportContent())
                        .Select(i => i.Topic).ToArray());

                    // Only include topics that exist.
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
            _consumer = Task.Run(ConsumerHandlerAsync, _cancelToken.Token);
        }
    }

    /// <summary>
    /// Keep consuming messages from Kafka until the service stops running.
    /// </summary>
    /// <returns></returns>
    private async Task ConsumerHandlerAsync()
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
            _cancelToken?.IsCancellationRequested == false)
        {
            this.Logger.LogDebug("Consumer thread is being cancelled");
            _cancelToken.Cancel();
        }
    }

    /// <summary>
    /// Import the content.
    /// Copy any file associated with source content.
    /// </summary>
    /// <param name="result"></param>
    /// <returns>Whether to continue with the next message.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<ConsumerAction> HandleMessageAsync(ConsumeResult<string, SourceContent> result)
    {
        try
        {
            this.Logger.LogInformation("Importing Content from Topic: {topic}, Uid: {key}", result.Topic, result.Message.Key);
            var model = result.Message.Value;

            // Only add if doesn't already exist.
            var exists = await _api.FindContentByUidAsync(model.Uid, model.Source);
            if (exists == null)
            {
                // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
                // TODO: Handle e-tag.
                var source = await _api.GetSourceForCodeAsync(model.Source);
                if (model.ProductId == 0)
                {
                    // Messages in Kafka are missing information, replace with best guess.
                    var ingests = await _api.GetIngestsForTopicAsync(result.Topic);
                    model.ProductId = ingests.FirstOrDefault()?.ProductId ?? throw new InvalidOperationException($"Unable to find an ingest for the topic '{result.Topic}'");
                }

                var content = new ContentModel()
                {
                    Status = Entities.ContentStatus.Draft, // TODO: Automatically publish based on Data Source config settings.
                    SourceId = source?.Id,
                    OtherSource = model.Source,
                    ContentType = model.ContentType,
                    ProductId = model.ProductId,
                    LicenseId = source?.LicenseId ?? 1,  // TODO: Default license by configuration.
                    SeriesId = null, // TODO: Provide default series from Data Source config settings.
                    OtherSeries = null, // TODO: Provide default series from Data Source config settings.
                    OwnerId = source?.OwnerId,
                    Headline = String.IsNullOrWhiteSpace(model.Title) ? "[TBD]" : model.Title,
                    Uid = result.Message.Value.Uid,
                    Page = "", // TODO: Provide default page from Data Source config settings.
                    Summary = String.IsNullOrWhiteSpace(model.Summary) ? "[TBD]" : model.Summary,
                    Body = "",
                    SourceUrl = model.Link,
                    PublishedOn = model.PublishedOn,
                };
                content = await _api.AddContentAsync(content);
                this.Logger.LogInformation("Content Imported.  Content ID: {id}, Pub: {published}", content?.Id, content?.PublishedOn);

                // Upload the file to the API.
                if (content != null && !String.IsNullOrWhiteSpace(model.FilePath))
                {
                    // TODO: Handle different storage locations.
                    // If the source content references a connection then fetch it to get the storage location of the file.
                    var fullPath = Path.Combine(_options.VolumePath, model.FilePath.MakeRelativePath());
                    if (File.Exists(fullPath))
                    {
                        var file = File.OpenRead(fullPath);
                        var fileName = Path.GetFileName(fullPath);
                        await _api.UploadFileAsync(content.Id, content.Version ?? 0, file, fileName);

                        // Send a Kafka message to the transcription topic
                        if (!String.IsNullOrWhiteSpace(_options.TranscriptionTopic))
                        {
                            await SendMessageAsync(content);
                        }
                    }
                    else
                    {
                        // TODO: Not sure if this should be considered a failure or not.
                        this.Logger.LogWarning("File not found.  Content ID: {id}, File: {path}", content.Id, fullPath);
                    }
                }
            }
            else
            {
                // TODO: Not sure if this should be considered a failure or not.
                // Content shouldn't exist already, it indicates unexpected scenario.
                this.Logger.LogWarning("Content already exists. Content Source: {Source}, UID: {Uid}", model.Source, model.Uid);
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
    /// Send message to kafka with updated transcription.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<DeliveryResult<string, TranscriptRequest>> SendMessageAsync(ContentModel content)
    {
        var result = await this.Producer.SendMessageAsync(_options.TranscriptionTopic, new TranscriptRequest(content, "ContentService"));
        if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka for {content.OtherSource}:{content.Uid}");
        return result;
    }
    #endregion
}
