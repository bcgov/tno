using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.ContentMigration.Config;
using TNO.Ches;
using TNO.Ches.Configuration;

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
    #endregion
}
