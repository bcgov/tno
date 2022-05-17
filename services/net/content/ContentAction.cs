using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Http;
using TNO.Services.Actions;
using TNO.Services.Content.Config;
using TNO.Models.Kafka;
using TNO.API.Areas.Services.Models.Content;

namespace TNO.Services.Content;

/// <summary>
/// ContentAction class, performs the Content ingestion action.
/// Fetch Content feed.
/// Send message to Kafka.
/// Inform api of new content.
/// </summary>
public class ContentAction : ServiceAction<ContentOptions>
{
    #region Variables
    private readonly IKafkaListener _kafka;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="kafka"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentAction(IApiService api, IKafkaListener kafka, IOptions<ContentOptions> options, ILogger<ContentAction> logger) : base(api, options)
    {
        _kafka = kafka;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <returns></returns>
    public override async Task PerformActionAsync(IServiceActionManager manager)
    {
        _logger.LogDebug("Performing Content service action for ...");

        await _kafka.ListenAsync<string, SourceContent>(async (result) =>
        {
            var content = new ContentModel()
            {
                Status = Entities.ContentStatus.Draft,
                WorkflowStatus = Entities.WorkflowStatus.Received,
                ContentTypeId = 1,
                MediaTypeId = 1,
                LicenseId = 1,
                SeriesId = null,
                OtherSeries = null,
                OwnerId = 1,
                DataSourceId = 1,
                Source = "",
                Headline = "",
                Uid = "",
                Page = "",
                Summary = "",
                Transcription = "",
                SourceUrl = "",
                PublishedOn = null,
                Actions = Array.Empty<ContentActionModel>(),
                Categories = Array.Empty<ContentCategoryModel>(),
                Tags = Array.Empty<ContentTagModel>(),
                TonePools = Array.Empty<ContentTonePoolModel>(),
                FileReferences = Array.Empty<FileReferenceModel>()
            };
            await this.Api.AddContentAsync(content);

        }, this.Options.Topics.ToArray());
    }
    #endregion
}
