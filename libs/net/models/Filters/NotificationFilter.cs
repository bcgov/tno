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
    #endregion

    #region Constructors
    public NotificationFilter() { }

    public NotificationFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.AlertOnIndex = filter.GetBoolNullValue(nameof(this.AlertOnIndex));
        this.NotificationType = filter.GetEnumNullValue<NotificationType>(nameof(this.NotificationType));
    }
    #endregion
}
