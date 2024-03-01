using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Command.Config;
using TNO.Ches;
using TNO.Ches.Configuration;

namespace TNO.Services.Command;

/// <summary>
/// CommandManager class, provides a way to manage the command service.
/// </summary>
public class CommandManager : IngestManager<CommandIngestActionManager, CommandOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="api"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CommandManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IngestManagerFactory<CommandIngestActionManager, CommandOptions> factory,
        IOptions<CommandOptions> options,
        ILogger<CommandManager> logger)
        : base(serviceProvider, api, chesService, chesOptions, factory, options, logger)
    {
    }
    #endregion
}
