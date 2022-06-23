using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Content.Config;
using TNO.API.Areas.Services.Models.Content;
using TNO.Models.Kafka;
using Confluent.Kafka;
using TNO.API.Areas.Editor.Models.Lookup;
using System.Text.Json;

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
    /// get - Lookup values from the API.
    /// </summary>
    public LookupModel? Lookups { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafka"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentManager(
        IKafkaListener kafka,
        IApiService api,
        IOptions<ContentOptions> options,
        ILogger<ContentManager> logger)
        : base(api, options, logger)
    {
        this.Consumer = kafka;
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
                    var topics = !String.IsNullOrWhiteSpace(_options.Topics) ? _options.GetTopics() : this.Lookups?.DataSources
                        .Where(ds => ds.IsEnabled &&
                            ds.ContentTypeId != null &&
                            !String.IsNullOrWhiteSpace(ds.Topic) &&
                            ds.Connection.ContainsKey("import") &&
                            ((JsonElement)ds.Connection["import"]).GetBoolean()).Select(ds => ds.Topic).ToArray() ?? Array.Empty<string>();

                    if (topics.Length != 0)
                    {
                        var tps = String.Join(',', topics);
                        this.Logger.LogInformation("Consuming topics: {tps}", tps);
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
        this.Logger.LogInformation("Importing Content from Topic: {Topic}, Uid: {Key}", result.Topic, result.Message.Key);

        // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
        // TODO: Handle e-tag.
        var source = await _api.GetDataSourceAsync(result.Topic) ?? throw new InvalidOperationException($"Failed to fetch data source for '{result.Topic}'");

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
            Source = result.Topic,
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
                var sourcePath = Path.Join(_options.ClipPath, result.Message.Value.FilePath);
                if (File.Exists(sourcePath))
                {
                    var file = File.OpenRead(sourcePath);
                    var fileName = Path.GetFileName(sourcePath);
                    await _api.UploadFileAsync(content.Id, content.Version ?? 0, file, fileName);
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
    #endregion
}
