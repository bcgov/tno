using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LinqKit;
using TNO.API.Areas.Editor.Models.Product;
using TNO.API.Areas.Editor.Models.Source;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Models.Extensions;
using TNO.Services.Actions;
using TNO.Services.ContentMigration.Config;
using TNO.Services.ContentMigration.Sources.Oracle;
using TNO.Services.ContentMigration.Migrators;
using TNO.Services.ContentMigration.Extensions;

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
    public ContentMigrationAction(MigrationSourceContext sourceContext, ContentMigratorFactory migratorFactory, IApiService api, IOptions<ContentMigrationOptions> options, ILogger<ContentMigrationAction> logger) : base(api, options, logger)
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
    /// <param name="creationDateOfNewsItem"></param>
    /// <returns></returns>
    private IQueryable<NewsItem> GetFilteredNewsItems(IQueryable<NewsItem> newsItems,
        System.Linq.Expressions.Expression<Func<NewsItem, bool>> predicate,
        int skip, int count, DateTime? lastRanOn, DateTime? importDateStart, DateTime? importDateEnd, DateTime? creationDateOfNewsItem)
    {
        // KGM : Do NOT remove the ItemDate null filter.  This excludes bad data
        predicate.And(ni => ni.ItemDateTime != null);

        // if no import filter dates are set
        if (!importDateStart.HasValue && !importDateEnd.HasValue) {
            // if the ingest has previously run, use the creationDateOfNewsItem as the dateFilter
            // if creationDateOfNewsItem is not set, use use DateTime.MinValue
            DateTime dateFilter = creationDateOfNewsItem ?? DateTime.MinValue;
            if(lastRanOn.HasValue) {
                dateFilter = new []{lastRanOn.Value, dateFilter}.Min();
            }
            predicate.And(ni => ni.ItemDateTime >= dateFilter);
        } else {
            DateTime dateFilterStart = importDateStart ?? DateTime.MinValue;
            DateTime dateFilterEnd = importDateEnd ?? DateTime.MaxValue;
            if(creationDateOfNewsItem.HasValue
                && (dateFilterStart <= creationDateOfNewsItem.Value)
                && (dateFilterEnd >= creationDateOfNewsItem.Value)) {
                    // if a date filter is set AND the creationDateOfNewsItem is set
                    // use the creationDateOfNewsItem as the start date ONLY if it's
                    // between the targeted start and end dates
                    dateFilterStart = new []{creationDateOfNewsItem.Value, dateFilterStart}.Max();
                }
            predicate.And(ni => (ni.ItemDateTime >= dateFilterStart) && (ni.ItemDateTime <= dateFilterEnd));
        }

        return newsItems.Where(predicate)
                .OrderBy(ni => ni.ItemDateTime) // oldest first
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
    public override async Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for ingest '{name}'", manager.Ingest.Name);
        IContentMigrator contentMigrator = _migratorFactory.GetContentMigrator(manager.Ingest.Name);

        API.Areas.Editor.Models.Lookup.LookupModel? lookups = await this.Api.GetLookupsAsync();

        var skip = 0;
        var countOfRecordsRetrieved = this.Options.MaxRecordsPerRetrieval;
        var maxIngestedRecords = Math.Max(this.Options.MaxRecordsPerIngest, this.Options.MaxRecordsPerRetrieval);

        DateTime? importDateStart = !string.IsNullOrEmpty(manager.Ingest.GetConfigurationValue("importDateStart")) ? manager.Ingest.GetConfigurationValue<DateTime>("importDateStart") : null;
        DateTime? importDateEnd = !string.IsNullOrEmpty(manager.Ingest.GetConfigurationValue("importDateEnd")) ? manager.Ingest.GetConfigurationValue<DateTime>("importDateEnd") : null;
        DateTime? creationDateOfLastImport = !string.IsNullOrEmpty(manager.Ingest.GetConfigurationValue("creationDateOfLastImport")) ? manager.Ingest.GetConfigurationValue<DateTime>("creationDateOfLastImport") : null;

        while ((countOfRecordsRetrieved > 0) && (skip < maxIngestedRecords))
        {
            try
            {
                var baseFilter = contentMigrator.GetBaseFilter();
                IQueryable<NewsItem> items = GetFilteredNewsItems(_sourceContext.NewsItems, baseFilter, skip, this.Options.MaxRecordsPerRetrieval, manager.Ingest.LastRanOn, importDateStart, importDateEnd, creationDateOfLastImport);
                countOfRecordsRetrieved = items.Count();

                await items.ForEachAsync(async newsItem =>
                {
                    await MigrateNewsItemAsync(manager, contentMigrator, lookups, newsItem);
                    creationDateOfLastImport = newsItem.GetPublishedDateTime();  // Don't convert to UTC as this compares back to a timestamp in the Oracle DB
                });
                await manager.UpdateIngestConfigAsync("creationDateOfLastImport", creationDateOfLastImport!.Value.ToString("yyyy-MM-dd h:mm:ss tt"));
                skip += countOfRecordsRetrieved;
            }
            catch (Exception)
            {
                Logger.LogError("Migration Failed on {skip}:{count}", skip, countOfRecordsRetrieved);
                // only update the DateTime.MinValue value if it was set
                if (creationDateOfLastImport != null)
                    await manager.UpdateIngestConfigAsync("creationDateOfLastImport", creationDateOfLastImport!.Value.ToString("yyyy-MM-dd h:mm:ss tt"));
                throw;
            }
        }

        Logger.LogInformation("Migration Complete");
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
    /// <returns></returns>
    public async Task MigrateNewsItemAsync(
        IIngestServiceActionManager manager,
        IContentMigrator contentMigrator,
        API.Areas.Editor.Models.Lookup.LookupModel? lookups,
        NewsItem newsItem)
    {
        if (lookups == null) {
            this.Logger.LogError("Lookups cannot be null");
            throw new ArgumentNullException(nameof(lookups));
        }

        try
        {
            SourceModel? source = contentMigrator.GetSourceMapping(lookups.Sources, newsItem.Source);
            if (source == null) {
                // if we don't have a custom mapping, fallback to our Ingest 'DEFAULT' mapping
                // TODO: KGM - When this happens, store the TNO 1.0 "Source" in "Other Source" field
                source = contentMigrator.GetSourceMapping(lookups.Sources, manager.Ingest.Source!.Code);
                if (source == null) {
                    this.Logger.LogWarning("Couldn't map to Source for NewsItem with source '{sourceName}'", newsItem.Source);
                    return;
                }
            }
            ProductModel? product = contentMigrator.GetProductMapping(lookups.Products, newsItem.Type);
            if (product == null) {
                this.Logger.LogWarning("Couldn't map to Product for NewsItem with type '{sourceName}'", newsItem.Type);
                return;
            }

            var sourceContent = contentMigrator.CreateSourceContent(lookups, source!, product, manager.Ingest.IngestType!.ContentType, newsItem);

            var reference = await this.FindContentReferenceAsync(source?.Code, sourceContent.Uid);
            if (reference == null)
            {
                reference = await this.Api.AddContentReferenceAsync(contentMigrator.CreateContentReference(source!, manager.Ingest.Topic, newsItem, sourceContent.Uid));
                Logger.LogInformation("Migrating content {RSN}:{Title}", newsItem.RSN, newsItem.Title);
                try {
                    if (newsItem.FilePath != null) {
                        string contentStagingFolderName = GetOutputPathPrefix(manager.Ingest);
                        await contentMigrator.CopyFileAsync(new Models.FileMigrationModel(newsItem.RSN, Path.GetDirectoryName(newsItem.FilePath)!, Path.GetFileName(newsItem.FilePath), newsItem.ContentType!), contentStagingFolderName);
                        newsItem.FilePath = Path.Combine(contentStagingFolderName, newsItem.FilePath);
                    }
                } catch(FileNotFoundException) {
                    // nothing we can do about it if the source content is archived/gone...
                    Logger.LogWarning("Migration source file content for RSN:{RSN} Path:{filePath} is missing", newsItem.RSN, newsItem.FilePath);
                }

                await ContentReceivedAsync(manager, reference, sourceContent);
            }
            else if (reference.Status == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddMinutes(5) < DateTime.UtcNow)
            {
                // If another process has it in progress only attempt to do an Migration if it's
                // more than an 5 minutes old. Assumption is that it is stuck.
                reference = await UpdateContentReferenceAsync(reference, WorkflowStatus.InProgress);
                Logger.LogInformation("Updating migrated content {RSN}:{Title}", newsItem.RSN, newsItem.Title);
            }
            else
            {
                // KGM - Found a ContentReference, but we are not stuck. Just skip???
                Logger.LogInformation("Item is pre-existing - skipping : {RSN}:{Title}", newsItem.RSN, newsItem.Title);
            }
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

    #endregion
}
