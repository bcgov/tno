using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Command.Config;

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
    /// <param name="api"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CommandManager(
        IApiService api,
        IngestManagerFactory<CommandIngestActionManager, CommandOptions> factory,
        IOptions<CommandOptions> options,
        ILogger<CommandManager> logger)
        : base(api, factory, options, logger)
    {
    }
    #endregion
}
