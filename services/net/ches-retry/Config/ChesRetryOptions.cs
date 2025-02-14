
using TNO.Services.Config;

namespace TNO.Services.ChesRetry.Config;

/// <summary>
/// ChesRetryOptions class, configuration options for notification service
/// </summary>
public class ChesRetryOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - Number of minutes before a retry can be attempted (default: 5)
    /// This will ensure a message must have been sent at least X minutes in the past before a new attempt will be made.
    /// </summary>
    public int RetryTimeLimit { get; set; } = 5;

    /// <summary>
    /// get/set - Number of minutes in the past that messages should be checked if they are sent (default: 60)
    /// This will make a request for all messages that were sent in this time frame.
    /// </summary>
    public int RetryTimeScope { get; set; } = 60;
    #endregion
}
