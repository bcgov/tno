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

    /// <summary>
    /// get/set - Default number of days stories are compared when removing duplicates across feeds.
    /// An ingest can override this with its 'dedupWindowDays' configuration.
    /// </summary>
    public int DedupWindowDays { get; set; } = 7;

    /// <summary>
    /// get/set - Default minimum headline word overlap (0 to 1) for two stories to be duplicates.
    /// An ingest can override this with its 'dedupHeadlineThreshold' configuration.
    /// </summary>
    public double DedupHeadlineThreshold { get; set; } = 0.8;

    /// <summary>
    /// get/set - Default minimum story text similarity (0.5 is unrelated, 1 is identical) for two stories to be duplicates.
    /// An ingest can override this with its 'dedupBodyThreshold' configuration.
    /// </summary>
    public double DedupBodyThreshold { get; set; } = 0.85;

    /// <summary>
    /// get/set - Headlines with fewer significant words than this never match on the headline alone.
    /// </summary>
    public int DedupMinHeadlineTokens { get; set; } = 3;

    /// <summary>
    /// get/set - Story text with fewer words than this is not compared.
    /// </summary>
    public int DedupMinBodyTokens { get; set; } = 40;

    /// <summary>
    /// get/set - Number of minutes before the duplicate index is reloaded from the API.
    /// </summary>
    public int DedupReloadMinutes { get; set; } = 60;

    /// <summary>
    /// get/set - Number of content references requested per page when loading the duplicate index.
    /// </summary>
    public int DedupLoadPageSize { get; set; } = 500;

    /// <summary>
    /// get/set - Maximum number of pages requested when loading the duplicate index.
    /// </summary>
    public int DedupLoadMaxPages { get; set; } = 40;
    #endregion
}
