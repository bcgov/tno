using TNO.Services.Config;

namespace TNO.Services.Syndication.Config;

/// <summary>
/// SyndicationOptions class, configuration options for syndication
/// </summary>
public class SyndicationOptions : IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The invalid characters and their expected format so this could be cleaned up
    /// before importing, and it should look like this - "{search1}:_{replace1}__{search2}:_{replace2}"
    /// </summary>
    public string InvalidEncodings { get; set; } = "";

    /// <summary>
    /// get - The key value set from InvalidEncodings ["{search1}:_{replace1}", "{search2}:_{replace2}"]
    /// </summary>
    public string[]? EncodingSets => InvalidEncodings?.Split("__", StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// get/set - Number of seconds before content is considered stuck in limbo and another message should be sent to Kafka.
    /// Defaults to 5 minutes.
    /// </summary>
    public int RetryAfterSeconds { get; set; } = 300;
    #endregion
}
