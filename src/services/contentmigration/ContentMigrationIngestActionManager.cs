using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Services.Actions.Managers;
using TNO.Services.ContentMigration.Config;

namespace TNO.Services.ContentMigration;

/// <summary>
/// ContentMigrationIngestActionManager class, provides a way to manage the Import ingestion process for this data source.
/// </summary>
public class ContentMigrationIngestActionManager : IngestActionManager<ContentMigrationOptions>
{
    #region Variables
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentMigrationIngestActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="api"></param>
    /// <param name="ches"></param>
    /// <param name="chesOptions"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentMigrationIngestActionManager(
        IngestModel ingest,
        IApiService api,
        IChesService ches,
        IOptions<ChesOptions> chesOptions,
        IIngestAction<ContentMigrationOptions> action,
        IOptions<ContentMigrationOptions> options,
        ILogger<IServiceActionManager> logger)
        : base(ingest, api, ches, chesOptions, action, options, logger)
    {
    }
    #endregion

    #region Methods
    #endregion
}
