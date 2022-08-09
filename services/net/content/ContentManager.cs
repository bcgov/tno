using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Content.Config;
using TNO.API.Areas.Services.Models.Content;
using TNO.Models.Kafka;
using Confluent.Kafka;
using TNO.API.Areas.Editor.Models.Lookup;
using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.Services.Content;

/// <summary>
/// ContentManager class, provides a Kafka Consumer service which imports content from all active topics.
/// </summary>
public class ContentManager : ServiceManager<ContentOptions>
{
    #region Variables
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer object.
    /// </summary>
    protected IKafkaListener Consumer { get; private set; }

    /// <summary>
    /// get - The kafka messenger service.
    /// </summary>
    protected IKafkaMessenger Producer { get; private set; }

    /// <summary>
    /// get - Lookup values from the API.
    /// </summary>
    public LookupModel? Lookups { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafkaListener"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentManager(
        IKafkaListener kafkaListener,
        IKafkaMessenger kafkaMessenger,
        IApiService api,
        IOptions<ContentOptions> options,
        ILogger<ContentManager> logger)
        : base(api, options, logger)
    {
        this.Consumer = kafkaListener;
        this.Producer = kafkaMessenger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Listen to active topics and import content.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var delay = _options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    // TODO: Handle e-tag.
                    // TODO: Update on some kind of interval.
                    this.Lookups = await _api.GetLookupsAsync();

                    // Listen to every enabled data source with a topic.
                    var topics = !String.IsNullOrWhiteSpace(_options.ContentTopics) ? _options.GetContentTopics() : this.Lookups?.DataSources
                        .Where(ds => ds.IsEnabled &&
                            ds.ContentTypeId > 0 &&
                            !String.IsNullOrWhiteSpace(ds.Topic) &&
                            ds.Connection.ContainsKey("import") &&
                            ((JsonElement)ds.Connection["import"]).GetBoolean()).Select(ds => ds.Topic).ToArray() ?? Array.Empty<string>();

                    if (topics.Length != 0)
                    {
                        var tps = String.Join(',', topics);
                        this.Logger.LogInformation("Consuming topics: {tps}", tps);

                        // TODO: Need to learn how to safely stop listening without losing content.  Every time a service goes down it will most likely lose content.
                        await this.Consumer.ListenAsync<string, SourceContent>(HandleMessageAsync, topics);

                        // Successful run clears any errors.
                        this.State.ResetFailures();
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
    /// Import the content.
    /// Copy any file associated with source content.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task HandleMessageAsync(ConsumeResult<string, SourceContent> result)
    {
        if (this.State.Status != ServiceStatus.Running)
        {
            this.Consumer.Stop();
            this.State.Stop();
        }

        this.Logger.LogInformation("Importing Content from Topic: {Topic}, Uid: {Key}", result.Topic, result.Message.Key);

        // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
        // TODO: Handle e-tag.
        var source = await _api.GetDataSourceAsync(result.Value.Source) ?? throw new InvalidOperationException($"Failed to fetch data source for '{result.Topic}'");

        if (source.ContentTypeId == null) throw new InvalidOperationException($"Data source not configured to import content correctly");

        var content = new ContentModel()
        {
            Status = Entities.ContentStatus.Draft, // TODO: Automatically publish based on Data Source config settings.
            WorkflowStatus = Entities.WorkflowStatus.Received, // TODO: Automatically extract based on lifecycle of content reference.
            ContentTypeId = source.ContentTypeId.Value,
            MediaTypeId = source.MediaTypeId,
            LicenseId = source.LicenseId,
            SeriesId = null, // TODO: Provide default series from Data Source config settings.
            OtherSeries = null, // TODO: Provide default series from Data Source config settings.
            OwnerId = source.OwnerId,
            DataSourceId = source.Id,
            Source = result.Value.Source,
            Headline = result.Message.Value.Title,
            Uid = result.Message.Value.Uid,
            Page = "", // TODO: Provide default page from Data Source config settings.
            Summary = result.Message.Value.Summary,
            Transcription = "",
            SourceUrl = result.Message.Value.Link,
            PublishedOn = result.Message.Value.PublishedOn,
        };

        // Only add if doesn't already exist.
        var exists = await _api.FindContentByUidAsync(content.Uid, content.Source);
        if (exists == null)
        {
            content = await _api.AddContentAsync(content);
            this.Logger.LogDebug("Content Imported.  Content ID: {Id}", content?.Id);

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
                    await SendMessageAsync(result.Message.Value, content);
                }
                else
                {
                    this.Logger.LogError("File not found.  Content ID: {Id}, File: {sourcePath}", content.Id, sourcePath);
                }
            }
        }
        else
        {
            this.Logger.LogDebug("Content already exists. Content Source: {Source}, UID: {Uid}", content.Source, content.Uid);
        }
    }

    /// <summary>
    /// Send message to kafka with updated transcription.
    /// </summary>
    /// <param name="sourceContent"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<Confluent.Kafka.DeliveryResult<string, SourceContent>> SendMessageAsync(SourceContent sourceContent, ContentModel content)
    {
        var result = await this.Producer.SendMessageAsync(_options.TranscriptionTopic, sourceContent);
        if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka for {content.Source}:{content.Uid}");
        return result;
    }
    #endregion
}
