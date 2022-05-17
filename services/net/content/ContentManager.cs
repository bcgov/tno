using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Content.Config;
using TNO.API.Areas.Services.Models.Content;
using TNO.Models.Kafka;
using Confluent.Kafka;
using TNO.API.Areas.Editor.Models.Lookup;

namespace TNO.Services.Content;

public class ContentManager : ServiceManager<ContentOptions>
{
    #region Variables
    #endregion

    #region Properties
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
        ILogger<ServiceManager<ContentOptions>> logger)
        : base(api, options, logger)
    {
        this.Consumer = kafka;
    }
    #endregion

    #region Methods
    public override async Task RunAsync()
    {
        var delay = _options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status != ServiceStatus.Running)
            {
                _logger.LogDebug("The service is not running '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    // TODO: Handle e-tag.
                    // TODO: Update on some kind of interval.
                    this.Lookups = await _api.GetLookupsAsync();

                    await this.Consumer.ListenAsync<string, SourceContent>(HandleMessageAsync, _options.Topics.ToArray());

                    // Successful run clears any errors.
                    this.State.ResetFailures();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Content manager had an unexpected failure.");
                    this.State.RecordFailure();
                }
            }

            // The delay ensures we don't have a run away thread.
            // With a minimum delay for all data source schedules, it could mean some data sources are pinged more often then required.
            _logger.LogDebug("Service sleeping for {delay} ms", delay);
            // await Thread.Sleep(new TimeSpan(0, 0, 0, delay));
            await Task.Delay(delay);
        }
    }

    private async Task HandleMessageAsync(ConsumeResult<string, SourceContent> result)
    {
        // TODO: Handle e-tag.
        var source = await _api.GetDataSourceAsync(result.Topic) ?? throw new InvalidOperationException($"Failed to fetch data source for '{result.Topic}'");

        var content = new ContentModel()
        {
            Status = Entities.ContentStatus.Draft,
            WorkflowStatus = Entities.WorkflowStatus.Received,
            ContentTypeId = source.ContentTypeId,
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
            Actions = Array.Empty<ContentActionModel>(),
            Categories = Array.Empty<ContentCategoryModel>(),
            Tags = Array.Empty<ContentTagModel>(),
            TonePools = Array.Empty<ContentTonePoolModel>(),
            FileReferences = Array.Empty<FileReferenceModel>()
        };
        await _api.AddContentAsync(content);
    }
    #endregion
}
