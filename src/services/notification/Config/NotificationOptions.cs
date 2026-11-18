
using TNO.Services.Config;

namespace TNO.Services.Notification.Config;

/// <summary>
/// NotificationOptions class, configuration options for notification service
/// </summary>
public class NotificationOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - The primary key to the alert action.
    /// </summary>
    public int AlertId { get; set; }

    /// <summary>
    /// get/set - Ignore any content that was indexed before this day offset.
    /// </summary>
    public int? IgnoreContentPublishedBeforeOffset { get; set; }

    /// <summary>
    /// get/set - The path files are uploaded to in the API.
    /// </summary>
    public string? UploadPath { get; set; }

    /// <summary>
    /// get/set - Maximum number of emails sent to CHES at the same time.
    /// </summary>
    public int MaxConcurrentSends { get; set; } = 10;

    /// <summary>
    /// get/set - Whether emails that could not be handed to CHES are sent back to Kafka to be tried again.
    /// </summary>
    public bool ResendOnFailure { get; set; } = true;

    /// <summary>
    /// get/set - Maximum number of times a failed email is sent back to Kafka.
    /// </summary>
    public int ResendAttemptLimit { get; set; } = 3;

    /// <summary>
    /// get/set - Number of milliseconds to wait before a failed email is tried again.
    /// </summary>
    public int ResendDelayMS { get; set; } = 30000;
    #endregion
}
