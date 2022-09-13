using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.Managers;
using TNO.Services.Content.Config;

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
                    var dataSources = (await _api.GetDataSourcesAsync()).ToArray();

                    // Listen to every enabled data source with a topic that is configured to produce content.
                    var topics = _options.GetContentTopics(dataSources
                        .Where(ds => ds.IsEnabled &&
                            ds.ContentTypeId > 0 &&
                            !String.IsNullOrWhiteSpace(ds.Topic))
                        .Select(ds => ds.Topic).ToArray());

                    // Only include topics that exist.
                    var kafkaTopics = this.KafkaAdmin.ListTopics();
                    topics = topics.Except(topics.Except(kafkaTopics)).ToArray();

                    if (topics.Length > 0)
                    {
                        if (!this.Consumer.IsReady) this.Consumer.Open();
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
        while (this.State.Status == ServiceStatus.Running && this.Consumer.IsReady)
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
    private bool ConsumerErrorHandler(object sender, ErrorEventArgs e)
    {
        this.State.RecordFailure();
        if (e.GetException() is ConsumeException ex)
        {
            return ex.Error.IsFatal;
        }

        // Inform the consumer it should stop.
        return this.State.Status != ServiceStatus.Running;
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
            this.Logger.LogDebug("Consumer thread is being cancelled");
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
    private async Task HandleMessageAsync(ConsumeResult<string, SourceContent> result)
    {
        this.Logger.LogInformation("Importing Content from Topic: {Topic}, Uid: {Key}", result.Topic, result.Message.Key);

        // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
        // TODO: Handle e-tag.
        if (String.IsNullOrWhiteSpace(result.Message.Value.Source)) throw new InvalidOperationException($"Message does not contain a source");
        var source = await _api.GetDataSourceAsync(result.Message.Value.Source) ?? throw new InvalidOperationException($"Failed to fetch data source for '{result.Message.Value.Source}'");
        if (source.ContentTypeId == 0) throw new InvalidOperationException($"Data source not configured to import content correctly");

        // Only add if doesn't already exist.
        var exists = await _api.FindContentByUidAsync(result.Message.Value.Uid, result.Message.Value.Source);
        if (exists == null)
        {
            var content = new ContentModel()
            {
                Status = Entities.ContentStatus.Draft, // TODO: Automatically publish based on Data Source config settings.
                WorkflowStatus = Entities.WorkflowStatus.Received, // TODO: Automatically extract based on lifecycle of content reference.
                ContentTypeId = source.ContentTypeId,
                MediaTypeId = source.MediaTypeId,
                LicenseId = source.LicenseId,
                SeriesId = null, // TODO: Provide default series from Data Source config settings.
                OtherSeries = null, // TODO: Provide default series from Data Source config settings.
                OwnerId = source.OwnerId,
                DataSourceId = source.Id,
                Source = result.Message.Value.Source,
                Headline = result.Message.Value.Title,
                Uid = result.Message.Value.Uid,
                Page = "", // TODO: Provide default page from Data Source config settings.
                Summary = result.Message.Value.Summary,
                Body = "",
                SourceUrl = result.Message.Value.Link,
                PublishedOn = result.Message.Value.PublishedOn,
            };
            content = await _api.AddContentAsync(content);
            this.Logger.LogInformation("Content Imported.  Content ID: {Id}", content?.Id);

            // Upload the file to the API.
            if (content != null && !String.IsNullOrWhiteSpace(result.Message.Value.FilePath))
            {
                // TODO: Handle different storage locations.
                // Remote storage locations may not be easily accessible by this service.
                var volumePath = source.GetConnectionValue("serviceType") switch
                {
                    "stream" => _options.CapturePath,
                    "clip" => _options.ClipPath,
                    _ => ""
                };
                var sourcePath = Path.Join(volumePath, result.Message.Value.FilePath);
                if (File.Exists(sourcePath))
                {
                    var file = File.OpenRead(sourcePath);
                    var fileName = Path.GetFileName(sourcePath);
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
                    this.Logger.LogWarning("File not found.  Content ID: {Id}, File: {sourcePath}", content.Id, sourcePath);
                }
            }
        }
        else
        {
            // TODO: Not sure if this should be considered a failure or not.
            // Content shouldn't exist already, it indicates unexpected scenario.
            this.Logger.LogWarning("Content already exists. Content Source: {Source}, UID: {Uid}", result.Message.Value.Source, result.Message.Value.Uid);
        }

        // Successful run clears any errors.
        this.State.ResetFailures();
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
        if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka for {content.Source}:{content.Uid}");
        return result;
    }
    #endregion
}
