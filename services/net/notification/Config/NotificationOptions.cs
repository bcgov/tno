
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
    #endregion
}
