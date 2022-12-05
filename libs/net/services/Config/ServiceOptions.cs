using System.ComponentModel.DataAnnotations;

namespace TNO.Services.Config;

/// <summary>
/// ServiceOptions class, configuration options for services.
/// </summary>
public class ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - Maximum number of sequential retries after a failure before the service stops running.
    /// </summary>
    public int MaxFailureLimit { get; set; } = 5;

    /// <summary>
    /// get/set - The number of attempts to retry a failed import.
    /// A retry that ultimately fails will still only count as a single failure for the service.
    /// </summary>
    public int RetryLimit { get; set; } = 3;

    /// <summary>
    /// get/set - Default millisecond delay between retry attempts of a failed attempt before attempting it again (default: 5 seconds).
    /// </summary>
    public int RetryDelayMS { get; set; } = 5000;

    /// <summary>
    /// get/set - Default millisecond delay between process cycle.  This stops run-away threads (default: 30 second).
    /// </summary>
    public int DefaultDelayMS { get; set; } = 30000;

    /// <summary>
    /// get/set - The URL to the API.
    /// </summary>
    [Required]
    public Uri ApiUrl { get; set; } = default!;

    /// <summary>
    /// get/set - The service timezone.
    /// </summary>
    public string TimeZone { get; set; } = "UTC";
    #endregion
}
