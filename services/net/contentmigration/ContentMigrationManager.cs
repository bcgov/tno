using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.ContentMigration.Config;

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
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentMigrationManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IngestManagerFactory<ContentMigrationIngestActionManager, ContentMigrationOptions> factory,
        IOptions<ContentMigrationOptions> options,
        ILogger<ContentMigrationManager> logger)
        : base(serviceProvider, api, factory, options, logger)
    {
    }
    #endregion
}
