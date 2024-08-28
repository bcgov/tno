
using TNO.Services.Config;

namespace TNO.Services.Reporting.Config;

/// <summary>
/// ReportingOptions class, configuration options for reporting service
/// </summary>
public class ReportingOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set- Whether a failure should make the report resend.
    /// </summary>
    public bool ResendOnFailure { get; set; }

    /// <summary>
    /// get/set - Number of attempts that will be made to resend a failed report before giving up.
    /// </summary>
    public int ResendAttemptLimit { get; set; } = 3;

    /// <summary>
    /// get/set - Whether to use the mail merge option.
    /// </summary>
    public bool UseMailMerge { get; set; } = true;

    /// <summary>
    /// get/set - Whether a failure to send to one subscriber should stop the process, or whether it should attempt to send to all subscribers before failing.
    /// </summary>
    public bool SendToAllSubscribersBeforeFailing { get; set; } = true;

    /// <summary>
    /// get/set - Number of retries allowed when a concurrency error occurs.
    /// </summary>
    public int RetryConcurrencyFailureLimit { get; set; } = 10;
    #endregion
}
