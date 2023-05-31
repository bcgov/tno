using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
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
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public ContentMigrationIngestActionManager(IngestModel ingest, IApiService api, IIngestAction<ContentMigrationOptions> action, IOptions<ContentMigrationOptions> options)
        : base(ingest, api, action, options)
    {
    }
    #endregion

    #region Methods
    #endregion
}
