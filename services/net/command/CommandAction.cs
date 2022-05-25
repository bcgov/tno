using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Command.Config;

namespace TNO.Services.Command;

/// <summary>
/// CommandAction class, performs the command ingestion action.
/// Fetch command feed.
/// Send message to Kafka.
/// Inform api of new content.
/// </summary>
public class CommandAction : CommandAction<CommandOptions>
{
    #region Variables
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CommandAction(IApiService api, IOptions<CommandOptions> options, ILogger<CommandAction> logger) : base(api, options, logger)
    {
    }
    #endregion

    #region Methods
    #endregion
}
