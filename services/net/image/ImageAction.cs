using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Models.Extensions;
using TNO.Models.Kafka;
using TNO.Services.Actions;
using TNO.Services.Actions.Managers;
using TNO.Services.Image.Config;

namespace TNO.Services.Image;

/// <summary>
/// ImageAction class, performs the image ingestion action.
/// Fetch image from data source location.
/// Send content reference to API.
/// Process image based on configuration.
/// Send message to Kafka.
/// Update content reference status.
/// </summary>
public class ImageAction : IngestAction<ImageOptions>
{
    #region Variables
    #endregion

    #region Properties
    /// <summary>
    /// get - The kafka messenger service.
    /// </summary>
    protected IKafkaMessenger Kafka { get; private set; }

    /// <summary>
    /// get - The logger.
    /// </summary>
    protected ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageAction, initializes with specified parameters.
    /// </summary>
    /// <param name="kafka"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageAction(IKafkaMessenger kafka, IApiService api, IOptions<ImageOptions> options, ILogger<ImageAction> logger) : base(api, options)
    {
        this.Kafka = kafka;
        this.Logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// Checks if a content reference has already been created for each image before deciding whether to import it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task PerformActionAsync<T>(IDataSourceIngestManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for data source '{Code}'", manager.DataSource.Code);

        // If the data source is configured to use a schedule then
        var content = CreateContentReference(manager.DataSource);

        await FetchImage(manager.DataSource);

        var reference = await this.Api.FindContentReferenceAsync(content.Source, content.Uid);
        var sendMessage = manager.DataSource.ContentTypeId > 0;

        if (reference == null)
        {
            reference = await this.Api.AddContentReferenceAsync(content);
        }
        else if (reference.WorkflowStatus == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddMinutes(2) < DateTime.UtcNow)
        {
            // If another process has it in progress only attempt to do an import if it's
            // more than an 2 minutes old. Assumption is that it is stuck.
            reference = await this.Api.UpdateContentReferenceAsync(reference);
        }
        else sendMessage = false;

        if (reference != null)
        {
            // await ProcessImage(manager.DataSource);
            await CopyImage(manager.DataSource);

            if (sendMessage)
            {
                var messageResult = await SendMessageAsync(manager.DataSource, reference);
                reference.Partition = messageResult.Partition;
                reference.Offset = messageResult.Offset;
            }

            reference.WorkflowStatus = (int)WorkflowStatus.Received;
            await this.Api.UpdateContentReferenceAsync(reference);
        }
    }

    /// <summary>
    /// Fetch the image from the remote data source based on configuration.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task FetchImage(DataSourceModel dataSource)
    {
        // TODO: use private keys in ./keys folder to connect to remote data source.
        // TODO: Fetch image from source data location.  Only continue if the image exists.
        // TODO: Eventually handle different data source locations based on config.
        throw new NotImplementedException();
    }

    /// <summary>
    /// Perform image processing based on configuration.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task ProcessImage(DataSourceModel dataSource)
    {
        // TODO: Process Image based on configuration.
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task CopyImage(DataSourceModel dataSource)
    {
        // TODO: Copy image to destination data location.
        // TODO: Eventually handle different destination data locations based on config.
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a content reference for this clip.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private ContentReferenceModel CreateContentReference(DataSourceModel dataSource)
    {
        var today = GetLocalDateTime(dataSource, DateTime.UtcNow);
        var publishedOn = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Local);
        var filename = dataSource.GetConnectionValue<string>("filename");
        return new ContentReferenceModel()
        {
            Source = dataSource.Code,
            Uid = $"{filename}-{publishedOn:yyyy-MM-dd-hh-mm-ss}",
            PublishedOn = publishedOn.ToUniversalTime(),
            Topic = dataSource.Topic,
            WorkflowStatus = (int)WorkflowStatus.InProgress
        };
    }

    /// <summary>
    /// Send message to kafka with new source content.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="reference"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<Confluent.Kafka.DeliveryResult<string, SourceContent>> SendMessageAsync(DataSourceModel dataSource, ContentReferenceModel reference)
    {
        var publishedOn = reference.PublishedOn ?? DateTime.UtcNow;
        var content = new SourceContent(SourceMediaType.Image, reference.Source, reference.Uid, $"{dataSource.Name} Frontpage", "", "", publishedOn.ToUniversalTime())
        {
            StreamUrl = dataSource.Parent?.GetConnectionValue("url") ?? "",
            FilePath = "some/path",
            Language = dataSource.Parent?.GetConnectionValue("language") ?? ""
        };
        var result = await this.Kafka.SendMessageAsync(reference.Topic, content);
        if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka for {reference.Source}:{reference.Uid}");
        return result;
    }

    /// <summary>
    /// Only return schedules that have passed and are within the 'ScheduleLimiter' configuration setting.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    protected IEnumerable<ScheduleModel> GetSchedules(DataSourceModel dataSource)
    {
        var now = GetLocalDateTime(dataSource, DateTime.UtcNow).TimeOfDay;
        return dataSource.DataSourceSchedules.Where(s =>
            s.Schedule != null &&
            s.Schedule.StopAt != null &&
            s.Schedule.StopAt.Value <= now
        ).Select(s => s.Schedule!);
    }

    /// <summary>
    /// Convert to timezone and return as local.
    /// Dates should be stored in the timezone of the data source.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    protected DateTime GetLocalDateTime(DataSourceModel dataSource, DateTime date)
    {
        return date.ToTimeZone(DataSourceIngestManager<ImageOptions>.GetTimeZone(dataSource, this.Options.TimeZone));
    }
    #endregion
}
