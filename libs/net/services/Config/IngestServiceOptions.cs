using System.ComponentModel.DataAnnotations;

namespace TNO.Services.Config;

/// <summary>
/// IngestServiceOptions class, configuration options for ingestion services.
/// </summary>
public class IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - Maximum number of sequential retries after a failure before the service stops running.
    /// </summary>
    public int MaxRetryLimit { get; set; } = 5;

    /// <summary>
    /// get/set - Default millisecond delay between process cycle.  This stops run-away threads (default: 1 second).
    /// </summary>
    public int DefaultDelayMS { get; set; } = 1000;

    /// <summary>
    /// get/set - The URL to the API.
    /// </summary>
    [Required]
    public Uri ApiUrl { get; set; } = default!;

    /// <summary>
    /// get/set - An array of media type this service will ingest.
    /// </summary>
    public string[] MediaTypes { get; set; } = Array.Empty<string>();
    #endregion
}
