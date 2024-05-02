using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.MediaType;
using TNO.API.Areas.Editor.Models.Source;
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
    enum ImportMigrationType
    {
        Unknown,
        Historic,
        Recent,
        RecentlyPublished
    }

    #region Variables
    private readonly MigrationSourceContext _sourceContext;
    private readonly ContentMigratorFactory _migratorFactory;
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
    /// <param name="newsItems"></param>
    /// <param name="predicate"></param>
    /// <param name="skip"></param>
    /// <param name="count"></param>
    /// <param name="lastRanOn"></param>
    /// <param name="importDateStart"></param>
    /// <param name="importDateEnd"></param>
    /// <param name="updatedDateOfNewsItem"></param>
    /// <param name="importOffsetInHours"></param>
    /// <returns></returns>
    private IQueryable<NewsItem> GetFilteredNewsItems(IQueryable<NewsItem> newsItems,
        System.Linq.Expressions.Expression<Func<NewsItem, bool>> predicate,
        int skip, int count, DateTime? lastRanOn,
        DateTime? importDateStart, DateTime? importDateEnd,
        DateTime? updatedDateOfNewsItem, double importOffsetInHours = 0)
    {
        // KGM : Do NOT remove these null filters.  They exclude bad data
        predicate = predicate.And(ni => ((ni.ItemDate != null) && (ni.Source != null) && (ni.Title != null)));

        DateTime offsetFromNow = DateTime.MaxValue;
        if (importOffsetInHours > 0)
        {
            // create an artificial buffer so that the migration isn't using most recent updates
            offsetFromNow = DateTime.Now.AddMinutes(-1 * 60 * importOffsetInHours);
        }

        // if no import filter dates are set
        if (!importDateStart.HasValue && !importDateEnd.HasValue)
        {
            // if the ingest has previously run, use the creationDateOfNewsItem as the dateFilter
            // if creationDateOfNewsItem is not set, use use DateTime.MinValue
            DateTime dateFilter = updatedDateOfNewsItem ?? DateTime.MinValue;
            if (lastRanOn.HasValue)
            {
                dateFilter = new[] { lastRanOn.Value, dateFilter }.Min();
            }
            if (offsetFromNow != DateTime.MaxValue)
            {
                // apply an artificial buffer so that the migration isn't using most recent updates
                predicate = predicate.And(ni => (ni.UpdatedOn >= dateFilter) && (ni.UpdatedOn <= offsetFromNow));
            }
            else
            {
                predicate = predicate.And(ni => ni.UpdatedOn >= dateFilter);
            }
        }
        else
        {
            DateTime dateFilterStart = importDateStart ?? DateTime.MinValue;
            DateTime dateFilterEnd = importDateEnd ?? DateTime.MaxValue;
            if (updatedDateOfNewsItem.HasValue
                && (dateFilterStart <= updatedDateOfNewsItem.Value)
                && (dateFilterEnd >= updatedDateOfNewsItem.Value))
            {
                // if a date filter is set AND the creationDateOfNewsItem is set
                // use the creationDateOfNewsItem as the start date ONLY if it's
                // between the targeted start and end dates
                dateFilterStart = new[] { updatedDateOfNewsItem.Value, dateFilterStart }.Max();
            }
            if ((dateFilterEnd != DateTime.MaxValue) || (offsetFromNow != DateTime.MaxValue))
            {
                dateFilterEnd = new[] { dateFilterEnd, offsetFromNow }.Min();
                predicate = predicate.And(ni => (ni.UpdatedOn >= dateFilterStart) && (ni.UpdatedOn <= dateFilterEnd));
            }
            else
            {
                predicate = predicate.And(ni => (ni.UpdatedOn >= dateFilterStart));
            }
        }

        return newsItems.Where(predicate)
                .OrderBy(ni => ni.UpdatedOn) // oldest first
                .ThenBy(ni => ni.RSN)
                .Skip(skip).Take(count);
    }
    /// <summary>
    ///
    /// </summary>
    /// <param name="newsItems"></param>
    /// <param name="predicate"></param>
    /// <param name="skip"></param>
    /// <param name="count"></param>
    /// <param name="importOffsetInHours"></param>
    /// <returns></returns>
    private IQueryable<NewsItem> GetRecentlyPublishedNewsItems(IQueryable<NewsItem> newsItems,
        System.Linq.Expressions.Expression<Func<NewsItem, bool>> predicate,
        int skip, int count, double importOffsetInHours = 0)
    {
        // KGM : Do NOT remove these null filters.  They exclude bad data
        predicate = predicate.And(ni => ((ni.ItemDate != null) && (ni.Source != null) && (ni.Title != null)));

        // KGM : Only return Published items
        predicate = predicate.And(ni => ((ni.Published)));

        DateTime offsetFromNow = DateTime.MaxValue;
        if (importOffsetInHours > 0)
        {
            // create an artificial buffer so that the migration isnt using most recent updates
            offsetFromNow = DateTime.Now.AddMinutes(-1 * 60 * importOffsetInHours);
        }

        // apply an artificial buffer so that the migration isnt using most recent updates
        predicate = predicate.And(ni => (ni.UpdatedOn >= offsetFromNow));

        return newsItems.Where(predicate)
                .OrderBy(ni => ni.UpdatedOn) // oldest first
                .ThenBy(ni => ni.RSN)
                .Skip(skip).Take(count);
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

        ImportMigrationType importMigrationType = manager.Ingest.GetConfigurationValue<ImportMigrationType>("importMigrationType", ImportMigrationType.Unknown);
        if (importMigrationType == ImportMigrationType.Unknown)
        {
            this.Logger.LogError("Error in Ingest [{ingestName}] config. 'importMigrationType' cannot be null.", manager.Ingest.Name);
            throw new ArgumentNullException(nameof(importMigrationType));
        }

        if (string.IsNullOrEmpty(this.Options.SupportedImportMigrationTypes))
        {
            this.Logger.LogError("Error in service config. 'SupportedImportMigrationTypes' cannot be null.");
            throw new ArgumentNullException(nameof(this.Options.SupportedImportMigrationTypes));
        }

        if (!this.Options.SupportedImportMigrationTypes.Split(',', StringSplitOptions.TrimEntries).Contains(importMigrationType.ToString()))
        {
            this.Logger.LogInformation("Skipping Ingest [{ingestName}]. Import Migration Type: [{migrationType}] not in supported list [{supportedMigrationTypes}]", manager.Ingest.Name, importMigrationType.ToString(), this.Options.SupportedImportMigrationTypes);
            return ServiceActionResult.Skipped;
        }

        // This service is configured to run this ingest, so reset failed attempts.
        await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);

        this.Logger.LogDebug("Performing ingestion service action for Ingest '{name}'", manager.Ingest.Name);
        IContentMigrator contentMigrator = _migratorFactory.GetContentMigrator(manager.Ingest.IngestType!.Name);

        API.Areas.Editor.Models.Lookup.LookupModel? lookups = await this.Api.GetLookupsAsync();

        var skip = 0;
        var countOfRecordsRetrieved = this.Options.MaxRecordsPerRetrieval;
        var maxIngestedRecords = Math.Max(this.Options.MaxRecordsPerIngest, this.Options.MaxRecordsPerRetrieval);
        var defaultTimeZone = GetTimeZone(manager.Ingest, this.Options.TimeZone);

        double migrationTimeOffsetInHours = 0;
        try
        {
            migrationTimeOffsetInHours = manager.Ingest.GetConfigurationValue<double>("migrationTimeOffsetInHours");
        }
        catch (Exception)
        {
            this.Logger.LogInformation("No migrationTimeOffsetInHours found for ingest '{name}'", manager.Ingest.Name);
        }

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

        DateTime? importDateStart = null;
        DateTime? importDateEnd = null;
        DateTime? creationDateOfLastImport = null;

        if ((importMigrationType == ImportMigrationType.Historic) || (importMigrationType == ImportMigrationType.Recent))
        {
            try
            {
                importDateStart = !string.IsNullOrEmpty(manager.Ingest.GetConfigurationValue("importDateStart")) ? manager.Ingest.GetConfigurationValue<DateTime>("importDateStart") : null;
            }
            catch (TNO.Core.Exceptions.ConfigurationException)
            {
                importDateStart = null;
            }
            try
            {
                importDateEnd = !string.IsNullOrEmpty(manager.Ingest.GetConfigurationValue("importDateEnd")) ? manager.Ingest.GetConfigurationValue<DateTime>("importDateEnd") : null;
            }
            catch (TNO.Core.Exceptions.ConfigurationException)
            {
                importDateEnd = null;
            }
            try
            {
                creationDateOfLastImport = manager.Ingest.CreationDateOfLastItem;
            }
            catch (Exception)
            {
                // migration has never been run before.
                this.Logger.LogInformation("CreationDateOfLastItem not found for ingest '{name}'", manager.Ingest.Name);
            }
        }

        while ((countOfRecordsRetrieved > 0) && (skip < maxIngestedRecords))
        {
            try
            {
                var baseFilter = contentMigrator.GetBaseFilter(manager.Ingest.IngestType.ContentType);

                IQueryable<NewsItem>? items = null;
                switch (importMigrationType)
                {
                    case ImportMigrationType.Historic:
                    case ImportMigrationType.Recent:
                        items = GetFilteredNewsItems(_sourceContext.NewsItems, baseFilter, skip, this.Options.MaxRecordsPerRetrieval, manager.Ingest.LastRanOn, importDateStart, importDateEnd, creationDateOfLastImport, migrationTimeOffsetInHours);
                        break;
                    case ImportMigrationType.RecentlyPublished:
                        maxIngestedRecords = int.MaxValue; // we need to ingest *ALL* the recently published items, so we cant set a chunk size :(
                        items = GetRecentlyPublishedNewsItems(_sourceContext.NewsItems, baseFilter, skip, this.Options.MaxRecordsPerRetrieval, migrationTimeOffsetInHours);
                        break;
                }

                if (items != null)
                {
                    this.Logger.LogDebug(items!.ToQueryString());
                    countOfRecordsRetrieved = items.Count();
                    this.Logger.LogDebug("Ingest [{name}] retrieved [{countOfRecordsRetrieved}] records", manager.Ingest.Name, countOfRecordsRetrieved);

                    if (countOfRecordsRetrieved == 0 && importDateEnd.HasValue)
                    {
                        this.Logger.LogDebug("Ingest [{name}] - no records prior to import end date filter [{importDateEnd}] records", manager.Ingest.Name, importDateEnd.Value.ToString("yyyy-MM-dd h:mm:ss tt"));
                        creationDateOfLastImport = importDateEnd.Value;
                    }

                    await items.ForEachAsync(async newsItem =>
                    {
                        await MigrateNewsItemAsync(manager, contentMigrator, lookups, newsItem, defaultTimeZone);
                        creationDateOfLastImport = newsItem.UpdatedOn;

                        // This ingest has just processed a story.
                        await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);
                    });
                }

                // might not have a date set here if the filter retrieved no records
                if (creationDateOfLastImport != null)
                    await manager.UpdateIngestStateCreationDateOfLastItemAsync(creationDateOfLastImport!.Value);

                skip += countOfRecordsRetrieved;
            }
            catch (Exception)
            {
                Logger.LogError("Migration Failed on {skip}:{count}", skip, countOfRecordsRetrieved);
                // only update the DateTime.MinValue value if it was set
                if (creationDateOfLastImport != null)
                    await manager.UpdateIngestStateCreationDateOfLastItemAsync(creationDateOfLastImport!.Value);
                throw;
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
    /// <param name="lookups"></param>
    /// <param name="newsItem"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    public async Task MigrateNewsItemAsync(
        IIngestActionManager manager,
        IContentMigrator contentMigrator,
        API.Areas.Editor.Models.Lookup.LookupModel? lookups,
        NewsItem newsItem,
        string defaultTimeZone)
    {
        if (lookups == null)
        {
            this.Logger.LogError("Lookups cannot be null");
            throw new ArgumentNullException(nameof(lookups));
        }

        try
        {
            SourceModel? source = contentMigrator.GetSourceMapping(lookups.Sources, newsItem.Source);
            if (source == null)
            {
                // if we don't have a custom mapping, fallback to our Ingest 'DEFAULT' mapping
                // TODO: KGM - When this happens, store the TNO 1.0 "Source" in "Other Source" field
                source = contentMigrator.GetSourceMapping(lookups.Sources, manager.Ingest.Source!.Code);
                if (source == null)
                {
                    this.Logger.LogWarning("Couldn't map to Source for NewsItem with source '{sourceName}'", newsItem.Source);
                    return;
                }
            }
            MediaTypeModel? mediaType = contentMigrator.GetMediaTypeMapping(lookups.MediaTypes, newsItem.Type);
            if (mediaType == null)
            {
                this.Logger.LogWarning("Couldn't map to Media Type for NewsItem with type '{sourceName}'", newsItem.Type);
                return;
            }

            var sourceContent = contentMigrator.CreateSourceContent(lookups, source!, mediaType, manager.Ingest.IngestType!.ContentType, newsItem, defaultTimeZone);
            if (this.Options.TagForMigratedContent != "")
            {
                sourceContent.Tags = sourceContent.Tags.Append(new TNO.Kafka.Models.Tag(this.Options.TagForMigratedContent, ""));
            }
            bool isNewSourceContent = false;
            bool isUpdatedSourceContent = false;
            var reference = await this.FindContentReferenceAsync(source?.Code, sourceContent.Uid);
            if (reference == null)
            {
                isNewSourceContent = true;
                reference = contentMigrator.CreateContentReference(source!, manager.Ingest.Topic, newsItem, sourceContent.Uid, defaultTimeZone);
            }
            else
            {
                // check if this is content, previously ingested by this service, but has been updated by an Editor in TNO
                DateTime originalLastUpdateDate = DateTime.MinValue;
                string originalSource = String.Empty;
                bool originalIsContentPublished = false;
                if (reference.Metadata.ContainsKey(ContentReferenceMetaDataKeys.MetadataKeyUpdatedOn))
                {
                    if (!DateTime.TryParse(reference.Metadata[ContentReferenceMetaDataKeys.MetadataKeyUpdatedOn].ToString(), out originalLastUpdateDate))
                        originalLastUpdateDate = DateTime.MinValue;
                }
                if (reference.Metadata.ContainsKey(ContentReferenceMetaDataKeys.MetadataKeyIngestSource))
                {
                    originalSource = reference.Metadata[ContentReferenceMetaDataKeys.MetadataKeyIngestSource].ToString() ?? String.Empty;
                }
                if (reference.Metadata.ContainsKey(ContentReferenceMetaDataKeys.MetadataKeyIsContentPublished))
                {
                    if (!Boolean.TryParse(reference.Metadata[ContentReferenceMetaDataKeys.MetadataKeyIsContentPublished].ToString(), out originalIsContentPublished))
                        originalIsContentPublished = false;
                }

                // IF this record was previously ingested from TNO by the Content Migration Service
                // AND ((it has been updated since it's original ingest)
                //  OR (the published status of the TNO items has changed))
                // THEN trigger an update to the content
                if ((sourceContent.UpdatedOn > originalLastUpdateDate) || (newsItem.Published != originalIsContentPublished))
                {
                    isUpdatedSourceContent = true;
                    reference.Status = (int)WorkflowStatus.InProgress;
                    // make sure the published status on the reference is up to date
                    reference.Metadata[ContentReferenceMetaDataKeys.MetadataKeyIsContentPublished] = newsItem.Published;
                    // update the stored UpdatedOn value
                    reference.Metadata[ContentReferenceMetaDataKeys.MetadataKeyUpdatedOn] = sourceContent.UpdatedOn?.ToString("yyyy-MM-dd h:mm:ss tt") ?? DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt");
                    // What about the worst case scenario: one Editor changes it in MMI and another Editor changes it in TNO?
                    Logger.LogInformation("Received updated content from TNO. Forcing an update to the MMI Content : {RSN}:{PublishedStatus}:{Title}", newsItem.RSN, newsItem.Published ? "PUBLISHED" : "UNPUBLISHED", newsItem.Title);
                }
            }

            if (isNewSourceContent)
            {
                reference = await this.Api.AddContentReferenceAsync(reference);
                Logger.LogInformation("Migrating content {RSN}:{PublishedStatus}:{Title}", newsItem.RSN, newsItem.Published ? "PUBLISHED" : "UNPUBLISHED", newsItem.Title);
            }

            if (isNewSourceContent || isUpdatedSourceContent)
            {
                try
                {
                    if (newsItem.FilePath != null)
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
                }
                catch (FileNotFoundException)
                {
                    // nothing we can do about it if the source content is archived/gone...
                    Logger.LogWarning("Migration source file content for RSN:{RSN} Path:{filePath} is missing", newsItem.RSN, newsItem.FilePath);
                }

                await ContentReceivedAsync(manager, reference, sourceContent);
            }
            else if (reference != null & reference?.Status == (int)WorkflowStatus.InProgress && reference?.UpdatedOn?.AddMinutes(5) < DateTime.UtcNow)
            {
                // If another process has it in progress only attempt to do an Migration if it's
                // more than an 5 minutes old. Assumption is that it is stuck.
                reference = await UpdateContentReferenceAsync(reference, WorkflowStatus.InProgress);
                Logger.LogInformation("Updating migrated content {RSN}:{PublishedStatus}:{Title}", newsItem.RSN, newsItem.Published ? "PUBLISHED" : "UNPUBLISHED", newsItem.Title);
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
