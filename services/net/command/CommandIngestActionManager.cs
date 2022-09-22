using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Services.Actions.Managers;
using TNO.Services.Command.Config;

namespace TNO.Services.Command;

/// <summary>
/// CommandIngestActionManager class, provides a way to manage the command ingestion process for this data source.
/// </summary>
public class CommandIngestActionManager : IngestActionManager<CommandOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandIngestActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public CommandIngestActionManager(IngestModel ingest, IApiService api, IIngestAction<CommandOptions> action, IOptions<CommandOptions> options)
        : base(ingest, api, action, options)
    {
    }
    #endregion

    #region Methods
    #endregion
}
