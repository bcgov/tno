using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Extensions;
using TNO.Models.Extensions;
using TNO.Services.Actions;
using TNO.Services.ContentMigration.Config;
using TNO.Core.Exceptions;
using TNO.Services.ContentMigration.Sources.Oracle;
using TNO.Services.ContentMigration.Migrators;
using LinqKit;

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
                .OrderByDescending(ni => ni.RSN)
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

        var lookups = await this.Api.GetLookupsAsync();
        IEnumerable<IngestModel> ingests = await this.Api.GetIngestsAsync();

        var skip = 0;
        var count = 50;
        DateTime? importDateStart = !string.IsNullOrEmpty(manager.Ingest.GetConfigurationValue("importDateStart")) ? manager.Ingest.GetConfigurationValue<DateTime>("importDateStart") : null;
        DateTime? importDateEnd = !string.IsNullOrEmpty(manager.Ingest.GetConfigurationValue("importDateEnd")) ? manager.Ingest.GetConfigurationValue<DateTime>("importDateEnd") : null;
        DateTime? creationDateOfLastImport = !string.IsNullOrEmpty(manager.Ingest.GetConfigurationValue("creationDateOfLastImport")) ? manager.Ingest.GetConfigurationValue<DateTime>("creationDateOfLastImport") : null;

        while (count > 0)
        {
            try
            {
                var baseFilter = contentMigrator.GetBaseFilter();
                IQueryable<NewsItem> items = GetFilteredNewsItems(_sourceContext.NewsItems, baseFilter, skip, count, manager.Ingest.LastRanOn, importDateStart, importDateEnd, creationDateOfLastImport);
                var lastNewsItem = items.Last();
                count = items.Count();
                skip += count;
                await items.ForEachAsync(async newsItem =>
                {
                    await contentMigrator.MigrateNewsItemAsync(manager, lookups, ingests, newsItem);
                    creationDateOfLastImport = newsItem.ItemDateTime;
                    if (newsItem.Equals(lastNewsItem) && (creationDateOfLastImport != null))
                    {
                        await manager.UpdateIngestConfigAsync("creationDateOfLastImport", creationDateOfLastImport);
                    }
                });

            }
            catch (Exception)
            {
                Logger.LogError("Migration Failed on {skip}:{count}", skip, count);
                // only update the DateTime.MinValue value if it was set
                if (creationDateOfLastImport != null)
                    await manager.UpdateIngestConfigAsync("creationDateOfLastImport", creationDateOfLastImport);
                throw;
            }
        }

        Logger.LogInformation("Migration Complete");
    }

    #endregion

    #region Support Methods
    #endregion
}
