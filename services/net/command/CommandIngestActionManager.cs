using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Ches;
using TNO.Ches.Configuration;
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
    /// <param name="api"></param>
    /// <param name="ches"></param>
    /// <param name="chesOptions"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CommandIngestActionManager(
        IngestModel ingest,
        IApiService api,
        IChesService ches,
        IOptions<ChesOptions> chesOptions,
        IIngestAction<CommandOptions> action,
        IOptions<CommandOptions> options,
        ILogger<IServiceActionManager> logger)
        : base(ingest, api, ches, chesOptions, action, options, logger)
    {
    }
    #endregion

    #region Methods
    #endregion
}
