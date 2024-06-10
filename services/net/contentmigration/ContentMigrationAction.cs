using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Models.Extensions;
using TNO.Services.Actions;
using TNO.Services.ContentMigration.Config;
using TNO.Services.ContentMigration.Migrators;
using TNO.Services.ContentMigration.Sources.Oracle;
using TNO.Services.ContentMigration.Sources.Oracle.Services;

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
    private readonly MigrationSourceContext _sourceContext;
    private readonly ContentMigratorFactory _migratorFactory;
    private API.Areas.Editor.Models.Lookup.LookupModel? _lookups;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentMigrationAction, initializes with specified parameters.
    /// </summary>
    /// <param name="sourceContext"></param>
    /// <param name="migratorFactory"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentMigrationAction(MigrationSourceContext sourceContext,
        ContentMigratorFactory migratorFactory,
        IApiService api,
        IOptions<ContentMigrationOptions> options,
        ILogger<ContentMigrationAction> logger) : base(api, options, logger)
    {
        _sourceContext = sourceContext;
        _migratorFactory = migratorFactory;
    }
    #endregion

    #region Methods
    /// <summary>
    ///
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="publishedOnly"></param>
    /// <param name="offsetHours"></param>
    /// <param name="importDateStart"></param>
    /// <param name="importDateEnd"></param>
    /// <param name="lastRanOn"></param>
    /// <returns></returns>
    private IQueryable<T> GetFilteredNewsItems<T>(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        bool publishedOnly,
        int? offsetHours,
        DateTime? importDateStart,
        DateTime? importDateEnd,
        DateTime? lastRanOn)
        where T : BaseNewsItem
    {
        // KGM : Do NOT remove these null filters.  They exclude bad data
        predicate = predicate.And(ni => ((ni.ItemDate != null) && (ni.Source != null) && (ni.Title != null)));

        var offsetDate = offsetHours > 0 ? DateTime.UtcNow.AddHours(-1 * offsetHours.Value) : (DateTime?)null;
        var minDate = lastRanOn.HasValue ? lastRanOn : importDateStart.HasValue ? importDateStart : offsetDate;

        if (publishedOnly)
            predicate = predicate.And(ni => ni.Published);
        if (minDate.HasValue)
            predicate = predicate.And(ni => ni.UpdatedOn >= minDate.Value.ToUniversalTime());
        if (importDateEnd.HasValue)
            predicate = predicate.And(ni => ni.UpdatedOn <= importDateEnd.Value.ToUniversalTime());

        return _sourceContext.Set<T>()
            .Include(m => m.Topics)
            .Include(m => m.Tones)
            .Where(predicate)
            .OrderBy(ni => ni.UpdatedOn) // oldest first
            .ThenBy(ni => ni.ItemTime) // oldest first
            .ThenBy(ni => ni.RSN);
    }

    /// <summary>
    /// Complete the query and make a request for the content.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="importMigrationType"></param>
    /// <param name="contentMigrator"></param>
    /// <param name="publishedOnly"></param>
    /// <param name="offsetHours"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="lastRunDate"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    private NewsItem[] GetNewsItems(
        TNO.Entities.ContentType contentType,
        ImportMigrationType importMigrationType,
        IContentMigrator contentMigrator,
        bool publishedOnly,
        int? offsetHours,
        DateTime? startDate,
        DateTime? endDate,
        DateTime? lastRunDate,
        int skip)
    {
        if (importMigrationType == ImportMigrationType.Historic)
        {
            var newsItemsQuery = contentMigrator.GetBaseFilter<HNewsItem>(contentType);
            var query = GetFilteredNewsItems<HNewsItem>(newsItemsQuery, publishedOnly, offsetHours, startDate, endDate, lastRunDate);
            query = query.Skip(skip).Take(this.Options.MaxRecordsPerRetrieval);

            this.Logger.LogDebug(query!.ToQueryString());
            return query.ToArray().Select(newsItem => (NewsItem)newsItem).ToArray();
        }
        else if (importMigrationType == ImportMigrationType.All)
        {
            var newsItemsQuery = contentMigrator.GetBaseFilter<AllNewsItem>(contentType);
            var query = GetFilteredNewsItems<AllNewsItem>(newsItemsQuery, publishedOnly, offsetHours, startDate, endDate, lastRunDate);
            query = query.Skip(skip).Take(this.Options.MaxRecordsPerRetrieval);

            this.Logger.LogDebug(query!.ToQueryString());
            return query.ToArray().Select(newsItem => (NewsItem)newsItem).ToArray();
        }
        else
        {
            var newsItemsQuery = contentMigrator.GetBaseFilter<NewsItem>(contentType);
            var query = GetFilteredNewsItems<NewsItem>(newsItemsQuery, publishedOnly, offsetHours, startDate, endDate, lastRunDate);
            query = query.Skip(skip).Take(this.Options.MaxRecordsPerRetrieval);

            this.Logger.LogDebug(query!.ToQueryString());
            return query.ToArray();
        }
    }

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
    /// <exception cref="ArgumentNullException"></exception>
    public override async Task<ServiceActionResult> PerformActionAsync<T>(IIngestActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        if (manager.Ingest.IngestType == null) throw new ConfigurationException("Ingest type cannot be null.");

        var importMigrationType = manager.Ingest.GetConfigurationValue<ImportMigrationType>("importMigrationType", ImportMigrationType.Unknown);
        if (importMigrationType == ImportMigrationType.Unknown)
            throw new ConfigurationException($"Error in Ingest [{manager.Ingest.Name}] config. 'importMigrationType' cannot be null.");

        if (string.IsNullOrEmpty(this.Options.SupportedImportMigrationTypes))
            throw new ConfigurationException("Error in service config. 'SupportedImportMigrationTypes' cannot be null.");

        if (!this.Options.SupportedImportMigrationTypes.Split(',', StringSplitOptions.TrimEntries).Contains(importMigrationType.ToString()))
        {
            this.Logger.LogDebug("Skipping Ingest [{ingestName}]. Import Migration Type: [{migrationType}] not in supported list [{supportedMigrationTypes}]", manager.Ingest.Name, importMigrationType.ToString(), this.Options.SupportedImportMigrationTypes);
            return ServiceActionResult.Skipped;
        }

        var publishedOnly = manager.Ingest.GetConfigurationValue<bool>("publishedOnly", false);
        if (this.Options.OnlyPublished && !publishedOnly)
        {
            this.Logger.LogDebug("Skipping Ingest [{ingestName}]. Only published content allowed", manager.Ingest.Name);
            return ServiceActionResult.Skipped;
        }

        // This service is configured to run this ingest, so reset failed attempts.
        await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);

        this.Logger.LogDebug("Performing ingestion service action for Ingest '{name}'", manager.Ingest.Name);
        var contentMigrator = _migratorFactory.GetContentMigrator(manager.Ingest.IngestType.Name) ?? throw new ConfigurationException($"Missing content migrator for ingest type '{manager.Ingest.IngestType?.Name}'.");

        _lookups = await this.Api.GetLookupsAsync() ?? throw new InvalidOperationException("Lookups cannot be null");

        var skip = 0;
        var maxRecordsPerRetrieval = this.Options.MaxRecordsPerRetrieval;
        var maxIngestedRecords = Math.Max(this.Options.MaxRecordsPerIngest, this.Options.MaxRecordsPerRetrieval);
        var defaultTimeZone = GetTimeZone(manager.Ingest, this.Options.TimeZone);

        if (manager.Ingest.SourceConnection != null)
        {
            string? sourceDbHostName = !string.IsNullOrEmpty(manager.Ingest.SourceConnection.GetConfigurationValue("hostname")) ? manager.Ingest.SourceConnection.GetConfigurationValue<string>("hostname") : null;
            int? sourceDbHostPort = !string.IsNullOrEmpty(manager.Ingest.SourceConnection.GetConfigurationValue("port")) ? manager.Ingest.SourceConnection.GetConfigurationValue<int>("port") : null;
            string? sourceDbSID = !string.IsNullOrEmpty(manager.Ingest.SourceConnection.GetConfigurationValue("sid")) ? manager.Ingest.SourceConnection.GetConfigurationValue<string>("sid") : null;
            string? sourceDbUserName = !string.IsNullOrEmpty(manager.Ingest.SourceConnection.GetConfigurationValue("username")) ? manager.Ingest.SourceConnection.GetConfigurationValue<string>("username") : null;
            string? sourceDbPassword = !string.IsNullOrEmpty(manager.Ingest.SourceConnection.GetConfigurationValue("password")) ? manager.Ingest.SourceConnection.GetConfigurationValue<string>("password") : null;

            if (sourceDbHostName != null && sourceDbHostPort != null && sourceDbSID != null && sourceDbUserName != null && sourceDbPassword != null)
            {
                _sourceContext.ChangeDatabaseConnectionString(OracleConnectionStringHelper.GetConnectionString(sourceDbUserName, sourceDbPassword, sourceDbHostName, sourceDbHostPort.Value, sourceDbSID));
            }
        }

        // Capture original so that we know if they change mid import.
        var originalImportDateStartValue = manager.Ingest.GetConfigurationValue<string?>("importDateStart", null);
        var originalImportDateEndValue = manager.Ingest.GetConfigurationValue<string?>("importDateEnd", null);
        var originalImportDateStart = !String.IsNullOrWhiteSpace(originalImportDateStartValue) ? DateTime.Parse(originalImportDateStartValue).ToTimeZone(defaultTimeZone) : (DateTime?)null;
        var originalImportDateEnd = !String.IsNullOrWhiteSpace(originalImportDateEndValue) ? DateTime.Parse(originalImportDateEndValue).ToTimeZone(defaultTimeZone) : (DateTime?)null;
        var originalOffsetHours = manager.Ingest.GetConfigurationValue<int?>("offsetHours", null);

        var creationDateOfLastImport = manager.Ingest.CreationDateOfLastItem;
        var retrievedRecords = 0;
        var continueFetchingRecords = true;
        while (continueFetchingRecords && (maxRecordsPerRetrieval > 0) && (skip < maxIngestedRecords))
        {
            if (manager.Ingest.IngestType == null) throw new ConfigurationException("Ingest type cannot be null.");

            var importDateStartValue = manager.Ingest.GetConfigurationValue<string?>("importDateStart", null);
            var importDateEndValue = manager.Ingest.GetConfigurationValue<string?>("importDateEnd", null);
            var importDateStart = !String.IsNullOrWhiteSpace(importDateStartValue) ? DateTime.Parse(importDateStartValue).ToTimeZone(defaultTimeZone) : (DateTime?)null;
            var importDateEnd = !String.IsNullOrWhiteSpace(importDateEndValue) ? DateTime.Parse(importDateEndValue).ToTimeZone(defaultTimeZone) : (DateTime?)null;
            var offsetHours = manager.Ingest.GetConfigurationValue<int?>("offsetHours", null);
            var importDelayMs = manager.Ingest.GetConfigurationValue<int?>("importDelayMs", null);
            var forceUpdate = manager.Ingest.GetConfigurationValue<bool>("forceUpdate", this.Options.ForceUpdate);

            // If the config changes, start from the beginning again.
            if (originalImportDateStart != importDateStart || originalImportDateEnd != importDateEnd || originalOffsetHours != offsetHours) skip = 0;

            try
            {
                var items = GetNewsItems(manager.Ingest.IngestType.ContentType, importMigrationType, contentMigrator, publishedOnly, offsetHours, importDateStart, importDateEnd, creationDateOfLastImport, skip);
                retrievedRecords = items.Length;
                this.Logger.LogDebug("Ingest [{name}] retrieved [{countOfRecordsRetrieved}] records", manager.Ingest.Name, retrievedRecords);

                DateTime? lastReceivedContentOn = null;
                if (retrievedRecords == 0 && importDateEnd.HasValue)
                {
                    this.Logger.LogDebug("Ingest [{name}] - no records prior to import end date filter [{importDateEnd}] records", manager.Ingest.Name, importDateEnd.Value.ToString("yyyy-MM-dd h:mm:ss tt"));
                    lastReceivedContentOn = importDateEnd.Value;
                }

                await items.ForEachAsync(async newsItem =>
                {
                    await MigrateNewsItemAsync(manager, contentMigrator, newsItem, defaultTimeZone, forceUpdate);
                    lastReceivedContentOn = newsItem.UpdatedOn;

                    // This ingest has just processed a story.
                    manager.Ingest = await manager.UpdateIngestStateFailedAttemptsAsync(0);
                });

                // Inform the ingest to pick up where it left off so that the next time it runs it will continue rather than restart.
                if (lastReceivedContentOn.HasValue && (!offsetHours.HasValue || offsetHours == 0))
                    manager.Ingest = await manager.UpdateIngestStateFailedAttemptsAsync(0, lastReceivedContentOn.Value);

                skip += retrievedRecords;
                if (retrievedRecords == 0) continueFetchingRecords = false;
                else if (importDelayMs.HasValue && importDelayMs > 0)
                {
                    // Artificial delay to reduce creating lag.
                    await Task.Delay(importDelayMs.Value, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Reached limit return to ingest manager.
                if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                    throw;

                Logger.LogError(ex, "Migration Failed on {skip}:{count}", skip, retrievedRecords);
                await manager.RecordFailureAsync(ex);
                await manager.SendEmailAsync($"Failed to ingest item for ingest '{manager.Ingest.Name}'", ex);
            }
        }

        if (skip >= maxIngestedRecords)
            this.Logger.LogInformation("Ingest [{name}] exited after reaching max ingest value [{maxIngest}]", manager.Ingest.Name, maxIngestedRecords);
        else
            this.Logger.LogInformation("Ingest [{name}] completed with [{countOfRecordsIngested}] records ingested", manager.Ingest.Name, skip);

        return ServiceActionResult.Success;
    }

    /// <summary>
    /// Attempt to migrate a single item from the historic system via api.
    /// Checks if a content reference has already been created for each item before deciding whether to migrate it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="contentMigrator"></param>
    /// <param name="newsItem"></param>
    /// <param name="defaultTimeZone"></param>
    /// <param name="forceUpdate"></param>
    /// <returns></returns>
    public async Task MigrateNewsItemAsync(
        IIngestActionManager manager,
        IContentMigrator contentMigrator,
        NewsItem newsItem,
        string defaultTimeZone,
        bool forceUpdate)
    {
        if (_lookups == null) return;

        try
        {
            var source = contentMigrator.GetSourceMapping(_lookups.Sources, newsItem.Source);
            if (source == null)
            {
                // if we don't have a custom mapping, fallback to our Ingest 'DEFAULT' mapping
                // TODO: KGM - When this happens, store the TNO 1.0 "Source" in "Other Source" field
                source = contentMigrator.GetSourceMapping(_lookups.Sources, manager.Ingest.Source?.Code);
                if (source == null)
                {
                    this.Logger.LogWarning("Couldn't map to Source for NewsItem with source '{sourceName}'", newsItem.Source);
                    return;
                }
            }
            var mediaType = contentMigrator.GetMediaTypeMapping(_lookups.MediaTypes, newsItem.Type);
            if (mediaType == null)
            {
                this.Logger.LogWarning("Couldn't map to Media Type for NewsItem with type '{sourceName}'", newsItem.Type);
                return;
            }

            var sourceContent = contentMigrator.CreateSourceContent(_lookups, source!, mediaType, manager.Ingest.IngestType!.ContentType, newsItem, defaultTimeZone);
            if (this.Options.TagForMigratedContent != "")
            {
                sourceContent.Tags = sourceContent.Tags.Append(new TNO.Kafka.Models.Tag(this.Options.TagForMigratedContent, ""));
            }

            // Fetch or create a content reference.  This is a locking mechanism to ensure only one process works on a single piece of content.
            var addOrUpdateContent = forceUpdate;
            var reference = await this.FindContentReferenceAsync(source?.Code, sourceContent.Uid);
            if (reference == null)
            {
                addOrUpdateContent = true;
                reference = contentMigrator.CreateContentReference(source!, manager.Ingest.Topic, newsItem, sourceContent.Uid, defaultTimeZone);
                reference = await this.Api.AddContentReferenceAsync(reference) ?? throw new InvalidOperationException("Failed to return content reference");
                Logger.LogInformation("Migrating content {RSN}:{PublishedStatus}:{Title}", newsItem.RSN, newsItem.Published ? "PUBLISHED" : "UNPUBLISHED", newsItem.Title);
            }
            else if (reference != null && reference.Status == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddMinutes(5) < DateTime.UtcNow)
            {
                // If another process has it in progress only attempt to do an Migration if it's
                // more than an 5 minutes old. Assumption is that it is stuck.
                addOrUpdateContent = true;
                reference.PublishedOn = sourceContent.PublishedOn;
                reference.SourceUpdateOn = sourceContent.UpdatedOn;
                reference = await UpdateContentReferenceAsync(reference, WorkflowStatus.InProgress);
                Logger.LogInformation("Updating migrated content {RSN}:{PublishedStatus}:{Title}", newsItem.RSN, newsItem.Published ? "PUBLISHED" : "UNPUBLISHED", newsItem.Title);
            }
            else if (reference != null)
            {
                // check if this is content previously ingested by this service, but has been updated by an Editor in TNO
                var originalLastUpdateDate = reference.Metadata!.GetDictionaryJsonValue<DateTime>(ContentReferenceMetaDataKeys.UpdatedOn, DateTime.MinValue);
                var originalIsContentPublished = reference.Metadata!.GetDictionaryJsonValue<bool>(ContentReferenceMetaDataKeys.IsContentPublished, false);
                var originalIsFeaturedStory = reference.Metadata!.GetDictionaryJsonValue<bool>(ContentReferenceMetaDataKeys.IsFeaturedStory, false);
                var originalIsTopStory = reference.Metadata!.GetDictionaryJsonValue<bool>(ContentReferenceMetaDataKeys.IsTopStory, false);
                var originalIsAlert = reference.Metadata!.GetDictionaryJsonValue<bool>(ContentReferenceMetaDataKeys.IsAlert, false);
                var originalIsCommentary = reference.Metadata!.GetDictionaryJsonValue<bool>(ContentReferenceMetaDataKeys.IsCommentary, false);
                var originalCommentaryTimeout = reference.Metadata!.GetDictionaryJsonValue<int?>(ContentReferenceMetaDataKeys.CommentaryTimeout, null);
                var originalEoDGroup = reference.Metadata!.GetDictionaryJsonValue<string?>(ContentReferenceMetaDataKeys.EoDGroup, null);
                var originalEoDCategory = reference.Metadata!.GetDictionaryJsonValue<string?>(ContentReferenceMetaDataKeys.EoDCategory, null);
                var originalTopicScore = reference.Metadata!.GetDictionaryJsonValue<int?>(ContentReferenceMetaDataKeys.TopicScore, null);
                var originalToneValue = reference.Metadata!.GetDictionaryJsonValue<int?>(ContentReferenceMetaDataKeys.ToneValue, null);

                // IF this record was previously ingested from TNO by the Content Migration Service
                // AND ((it has been updated since it's original ingest)
                //  OR (the published status of the TNO items has changed))
                // THEN trigger an update to the content
                if ((sourceContent.UpdatedOn > originalLastUpdateDate)
                    || (newsItem.Published != originalIsContentPublished)
                    || (newsItem.FrontPageStory != originalIsFeaturedStory)
                    || (newsItem.WapTopStory != originalIsTopStory)
                    || (newsItem.Alert != originalIsAlert)
                    || (newsItem.Commentary != originalIsCommentary)
                    || (newsItem.CommentaryTimeout != originalCommentaryTimeout)
                    || (newsItem.EodGroup != originalEoDGroup)
                    || (newsItem.EodCategory != originalEoDCategory)
                    || (newsItem.Topics.FirstOrDefault()?.Score != originalTopicScore)
                    || (newsItem.Tones.FirstOrDefault()?.ToneValue != originalToneValue))
                {
                    addOrUpdateContent = true;

                    // Update the content reference so that we can compare future fetches.
                    reference.PublishedOn = sourceContent.PublishedOn;
                    reference.SourceUpdateOn = sourceContent.UpdatedOn;
                    reference.Metadata[ContentReferenceMetaDataKeys.IsContentPublished] = newsItem.Published;
                    reference.Metadata[ContentReferenceMetaDataKeys.UpdatedOn] = sourceContent.UpdatedOn?.ToString("yyyy-MM-dd h:mm:ss tt") ?? DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt");
                    reference.Metadata[ContentReferenceMetaDataKeys.IsFeaturedStory] = newsItem.FrontPageStory;
                    reference.Metadata[ContentReferenceMetaDataKeys.IsTopStory] = newsItem.WapTopStory;
                    reference.Metadata[ContentReferenceMetaDataKeys.IsAlert] = newsItem.Alert;
                    reference.Metadata[ContentReferenceMetaDataKeys.IsCommentary] = newsItem.Commentary;
                    reference.Metadata[ContentReferenceMetaDataKeys.CommentaryTimeout] = newsItem.CommentaryTimeout;
                    reference.Metadata[ContentReferenceMetaDataKeys.EoDGroup] = newsItem.EodGroup;
                    reference.Metadata[ContentReferenceMetaDataKeys.EoDCategory] = newsItem.EodCategory;
                    reference.Metadata[ContentReferenceMetaDataKeys.TopicScore] = newsItem.Topics.FirstOrDefault()?.Score;
                    reference.Metadata[ContentReferenceMetaDataKeys.ToneValue] = newsItem.Tones.FirstOrDefault()?.ToneValue;

                    reference = await UpdateContentReferenceAsync(reference, WorkflowStatus.InProgress);
                    // What about the worst case scenario: one Editor changes it in MMI and another Editor changes it in TNO?
                    Logger.LogInformation("Received updated content from TNO. Forcing an update to the MMI Content : {RSN}:{PublishedStatus}:{Title}", newsItem.RSN, newsItem.Published ? "PUBLISHED" : "UNPUBLISHED", newsItem.Title);
                }
            }

            // Send the source content to Kafka so that it can be added or updated in MMI.
            if (addOrUpdateContent)
            {
                await LinkFile(manager, contentMigrator, newsItem, sourceContent);
                await ContentReceivedAsync(manager, reference, sourceContent);
            }
            else
            {
                Logger.LogInformation("No action taken.  Not new, updated or stuck.  {RSN}:{PublishedStatus}:{Title}", newsItem.RSN, newsItem.Published ? "PUBLISHED" : "UNPUBLISHED", newsItem.Title);
            }
        }
        catch (Exception ex)
        {
            // Reached limit return to ingest manager.
            if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                throw;

            this.Logger.LogError(ex, "Failed to ingest item for ingest '{name}'", manager.Ingest.Name);
            await manager.RecordFailureAsync(ex);
            await manager.SendEmailAsync($"Failed to ingest item for ingest '{manager.Ingest.Name}'", ex);
        }
    }

    /// <summary>
    /// Link the associated file if it exists.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="contentMigrator"></param>
    /// <param name="newsItem"></param>
    /// <param name="sourceContent"></param>
    /// <returns></returns>
    private async Task LinkFile(
        IIngestActionManager manager,
        IContentMigrator contentMigrator,
        NewsItem newsItem,
        Kafka.Models.SourceContent sourceContent)
    {
        if (newsItem.FilePath != null)
        {
            try
            {
                if (string.IsNullOrEmpty(newsItem.ContentType))
                {
                    string ext = Path.GetExtension(newsItem.FilePath);
                    switch (ext.ToUpper())
                    {
                        case ".DOC":
                            newsItem.ContentType = "application/msword";
                            break;
                        case ".GIF":
                            newsItem.ContentType = "image/gif";
                            break;
                        case ".JPEG":
                        case ".JPG":
                            newsItem.ContentType = "image/jpeg";
                            break;
                        case ".HTM":
                            newsItem.ContentType = "text/html";
                            break;
                        case ".MOV":
                        case ".MP4":
                        case ".M4A":
                            newsItem.ContentType = "video/quicktime";
                            break;
                        case ".PDF":
                            newsItem.ContentType = "application/pdf";
                            break;
                        case ".TXT":
                            newsItem.ContentType = "text/plain";
                            break;
                    }
                }

                if (string.IsNullOrEmpty(newsItem.ContentType))
                {
                    Logger.LogWarning("Skipping file migration, ContentType is missing {RSN}:{PublishedStatus}:{Title}", newsItem.RSN, newsItem.Published ? "PUBLISHED" : "UNPUBLISHED", newsItem.Title);
                }
                else
                {
                    string contentStagingFolderName = GetOutputPathPrefix(manager.Ingest);
                    await contentMigrator.CopyFileAsync(new Models.FileMigrationModel(newsItem.RSN, Path.GetDirectoryName(newsItem.FilePath)!, Path.GetFileName(newsItem.FilePath), newsItem.ContentType!), contentStagingFolderName);
                    sourceContent.FilePath = Path.Combine(contentStagingFolderName, newsItem.FilePath);
                    Logger.LogInformation("Migrating file associated with content content {RSN}:{Title}:[{filePath}]", newsItem.RSN, newsItem.Title, sourceContent.FilePath);
                }
            }
            catch (FileNotFoundException)
            {
                // nothing we can do about it if the source content is archived/gone...
                Logger.LogWarning("Migration source file content for RSN:{RSN} Path:{filePath} is missing", newsItem.RSN, newsItem.FilePath);
            }
        }
    }

    #endregion

    #region Support Methods
    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected static string GetOutputPathPrefix(API.Areas.Services.Models.Ingest.IngestModel ingest)
    {
        return ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "";
    }

    /// <summary>
    /// Get the timezone arguments from the connection settings.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetTimeZone(API.Areas.Services.Models.Ingest.IngestModel ingest, string defaultTimeZone)
    {
        var value = ingest.GetConfigurationValue("timeZone");
        return String.IsNullOrWhiteSpace(value) ? defaultTimeZone : value;
    }
    #endregion
}
