using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.Models.Filters;

/// <summary>
/// NotificationFilter class, provides a model for searching content.
/// </summary>
public class NotificationFilter : PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - Only include notifications that alert on index.
    /// </summary>
    public bool? AlertOnIndex { get; set; }

    /// <summary>
    /// get/set - The notification type.
    /// </summary>
    public NotificationType? NotificationType { get; set; }

    /// <summary>
    /// get/set - The notification status.
    /// </summary>
    public NotificationStatus? NotificationStatus { get; set; }

    /// <summary>
    /// get/set - The owner of the notification.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Only include content owned by this user.
    /// </summary>
    public int? SubscriberUserId { get; set; }

    /// <summary>
    /// get/set - Only include notifications that are enabled.
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// get/set - An array of notification ids to return.
    /// </summary>
    public int[]? Ids { get; set; }
    #endregion

    #region Constructors
    public NotificationFilter() { }

    public NotificationFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.AlertOnIndex = filter.GetBoolNullValue(nameof(this.AlertOnIndex));
        this.NotificationType = filter.GetEnumNullValue<NotificationType>(nameof(this.NotificationType));
        this.OwnerId = filter.GetIntNullValue(nameof(this.OwnerId));
        this.SubscriberUserId = filter.GetIntNullValue(nameof(this.SubscriberUserId));
        this.IsEnabled = filter.GetBoolNullValue(nameof(this.IsEnabled));
        this.Ids = filter.GetIntArrayValue(nameof(this.Ids));
        this.NotificationStatus = filter.GetEnumNullValue<NotificationStatus>(nameof(this.NotificationStatus));
    }
    #endregion
}
