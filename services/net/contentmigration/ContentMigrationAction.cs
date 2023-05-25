using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Models.Extensions;
using TNO.Kafka.Models;
using TNO.Services.Actions;
using TNO.Services.ContentMigration.Config;
using TNO.Core.Exceptions;
using System.Text.RegularExpressions;

namespace TNO.Services.ContentMigration;

/// <summary>
/// ContentMigrationAction class, performs the Migration ingestion action.
/// Fetch Migration from ingest location.
/// Send content reference to API.
/// Process Migration based on configuration.
/// Send message to Kafka.
/// Update content reference status.
/// </summary>
public class ContentMigrationAction : IngestAction<ContentMigrationOptions>
{
    #region Variables
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentMigrationAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentMigrationAction(IApiService api, IOptions<ContentMigrationOptions> options, ILogger<ContentMigrationAction> logger) : base(api, options, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// Checks if a content reference has already been created for each migration item before deciding whether to migrate it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public override async Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for ingest '{name}'", manager.Ingest.Name);

        var skip = 0;
        var count = 0;

        while (count > 0)
        {
            try
            {
                // attempt to retrieve a batch of non-migrated news items

                // if there news items to be migrated in the batch
                // iterate over each item in the batch

                // determine what "type" of news item the current item is
                // handle it with a strategy specific to that type

                try
                {
                    // KGM - fixed values in place until we bring in NewsItem entity
                    var newsItemWebPath = "newsItem.WebPath";
                    var newsItemRSN = "NewsItem.RSN";
                    var newsItemTitle = "NewsItem.Title";

                    // KGM: does uid exist for all newsItem types in old system and does this correlate to new system?
                    // KGM: for soft-launch - how do we avoid duplication if MMIA is ingest from source *AND* historic?
                    // var uid = newsItem.WebPath ?? $"{newsItem.RSN}";
                    var uid = newsItemWebPath ?? $"{newsItemRSN}";
                    var reference = await this.FindContentReferenceAsync(manager.Ingest.Source?.Code, uid);
                    if (reference == null)
                    {
                        reference = await this.Api.AddContentReferenceAsync(CreateContentReference(manager.Ingest, uid));
                        // Logger.LogInformation("Migrating content {RSN}:{Title}", newsItem.RSN, newsItem.Title);
                        Logger.LogInformation("Migrating content {RSN}:{Title}", newsItemRSN, newsItemTitle);
                    }
                    else if (reference.Status == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddMinutes(5) < DateTime.UtcNow)
                    {
                        // If another process has it in progress only attempt to do an Migration if it's
                        // more than an 5 minutes old. Assumption is that it is stuck.
                        reference = await UpdateContentReferenceAsync(reference, WorkflowStatus.InProgress);
                        // Logger.LogInformation("Updating migrated content {RSN}:{Title}", newsItem.RSN, newsItem.Title);
                        Logger.LogInformation("Updating migrated content {RSN}:{Title}", newsItemRSN, newsItemTitle);
                    }

                    reference = await FindContentReferenceAsync(reference?.Source, reference?.Uid);
                    if (reference != null) await ContentReceivedAsync(manager, reference, CreateSourceContent(manager.Ingest, reference));
                }
                catch (Exception ex)
                {
                    // Reached limit return to ingest manager.
                    if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                        throw;

                    this.Logger.LogError(ex, "Failed to ingest item for ingest '{name}'", manager.Ingest.Name);
                    await manager.RecordFailureAsync();
                }


            }
            catch (Exception)
            {
                Logger.LogError("Migration Failed on {skip}:{count}", skip, count);
                throw;
            }
        }

        Logger.LogInformation("Migration Complete");
    }

    /// <summary>
    /// Create a content reference for this clip.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private ContentReferenceModel CreateContentReference(IngestModel ingest, string filename)
    {
        var publishedOnExpression = ingest.GetConfigurationValue("publishedOnExpression");

        var today = GetDateTimeForTimeZone(ingest);
        DateTime publishedOn;
        if (!String.IsNullOrWhiteSpace(publishedOnExpression))
        {
            try
            {
                var match = Regex.Match(filename, publishedOnExpression, RegexOptions.Singleline);
                var year = String.IsNullOrWhiteSpace(match.Groups["year"].Value) ? today.Year : int.Parse(match.Groups["year"].Value);
                var month = String.IsNullOrWhiteSpace(match.Groups["month"].Value) ? today.Month : int.Parse(match.Groups["month"].Value);
                var day = String.IsNullOrWhiteSpace(match.Groups["day"].Value) ? today.Day : int.Parse(match.Groups["day"].Value);
                var hour = String.IsNullOrWhiteSpace(match.Groups["hour"].Value) ? today.Hour : int.Parse(match.Groups["hour"].Value);
                var minute = String.IsNullOrWhiteSpace(match.Groups["minute"].Value) ? today.Minute : int.Parse(match.Groups["minute"].Value);
                var second = String.IsNullOrWhiteSpace(match.Groups["second"].Value) ? today.Second : int.Parse(match.Groups["second"].Value);
                publishedOn = new DateTime(year, month, day, hour, minute, second, today.Kind);

                // If the published on date is greater than today we will assume it's in the morning.
                if (today < publishedOn) publishedOn = publishedOn.Add(new TimeSpan(0));
            }
            catch (Exception ex)
            {
                // Essentially ignore the error and set the published on date to today.
                this.Logger.LogError(ex, "Regex failed for 'publishedOnExpression': {regex}", publishedOnExpression);
                publishedOn = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);
            }
        }
        else
            publishedOn = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);

        return new ContentReferenceModel()
        {
            Source = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code."),
            Uid = $"{filename}",
            PublishedOn = this.ToTimeZone(publishedOn, ingest).ToUniversalTime(),
            Topic = ingest.Topic,
            Status = (int)WorkflowStatus.InProgress
        };
    }

    /// <summary>
    /// Send message to kafka with new source content.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="reference"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private SourceContent? CreateSourceContent(IngestModel ingest, ContentReferenceModel reference)
    {
        var publishedOn = reference.PublishedOn ?? DateTime.UtcNow;
        var contentType = ingest.IngestType?.ContentType ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing ingest content type.");
        var content = new SourceContent(
            this.Options.DataLocation,
            reference.Source,
            contentType,
            ingest.ProductId,
            reference.Uid,
            $"{ingest.Name} Frontpage",
            "",
            "",
            publishedOn.ToUniversalTime(),
            ingest.GetConfigurationValue<bool>("publish"))
        {
            StreamUrl = ingest.GetConfigurationValue("url"),
            FilePath = (ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "")
                .CombineWith($"{ingest.Source?.Code}/{GetDateTimeForTimeZone(ingest):yyyy-MM-dd}/", reference.Uid),
            Language = ingest.GetConfigurationValue("language")
        };
        return content;
    }

    #endregion

    #region Support Methods

    #endregion
}
