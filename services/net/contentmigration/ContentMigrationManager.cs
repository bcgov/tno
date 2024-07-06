using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Models.Extensions;
using TNO.Services.ContentMigration.Config;
using TNO.Services.Managers;

namespace TNO.Services.ContentMigration;

/// <summary>
/// ContentMigrationManager class, provides a way to manage the Import service.
/// </summary>
public class ContentMigrationManager : IngestManager<ContentMigrationIngestActionManager, ContentMigrationOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentMigrationManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="api"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentMigrationManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IngestManagerFactory<ContentMigrationIngestActionManager, ContentMigrationOptions> factory,
        IOptions<ContentMigrationOptions> options,
        ILogger<ContentMigrationManager> logger)
        : base(serviceProvider, api, chesService, chesOptions, factory, options, logger)
    {
    }

    /// <summary>
    /// Get the ingests for this service.
    /// </summary>
    /// <returns></returns>
    public override async Task<IEnumerable<IngestModel>> GetIngestsAsync()
    {
        if (string.IsNullOrEmpty(this.Options.SupportedImportMigrationTypes))
            throw new ConfigurationException("Error in service config. 'SupportedImportMigrationTypes' cannot be null.");

        var ingests = await base.GetIngestsAsync();
        var validIngests = new List<IngestModel>();

        // Remove any ingest configurations that are not supported by this specific service instance.
        foreach (var ingest in ingests)
        {
            var importMigrationType = ingest.GetConfigurationValue("importMigrationType", ImportMigrationType.Unknown);
            var publishedOnly = ingest.GetConfigurationValue("publishedOnly", false);

            if (importMigrationType == ImportMigrationType.Unknown)
            {
                this.Logger.LogWarning("Error in Ingest [{ingest.Name}] config. 'importMigrationType' cannot be null.", ingest.Name);
            }
            else if (!this.Options.SupportedImportMigrationTypes.Split(',', StringSplitOptions.TrimEntries).Contains(importMigrationType.ToString()))
            {
                this.Logger.LogDebug("Skipping Ingest [{ingestName}]. Import Migration Type: [{migrationType}] not in supported list [{supportedMigrationTypes}]", ingest.Name, importMigrationType.ToString(), this.Options.SupportedImportMigrationTypes);
            }
            else if (this.Options.OnlyPublished && !publishedOnly)
            {
                this.Logger.LogDebug("Skipping Ingest [{ingestName}]. Only published content allowed", ingest.Name);
            }
            else
            {
                validIngests.Add(ingest);
            }
        }

        return validIngests;
    }
    #endregion
}
