using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;

namespace TNO.Models.Filters;

/// <summary>
/// DashboardFilter class, provides a model for searching content.
/// </summary>
public class DashboardFilter : PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - Only include content owned by this user.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Only include this notification.
    /// </summary>
    public int? NotificationId { get; set; }

    /// <summary>
    /// get/set - The name of the report.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// get/set - Search for reports with this keyword in the name, or owner name.
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// get/set - Whether the report is public.
    /// </summary>
    public bool? IsPublic { get; set; }

    /// <summary>
    /// get/set - Whether the report is enabled.
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// get/set - Only include reports that were last sent on or after this date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// get/set - Only include reports that were last sent on before this date.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// get/set - Only include report instances with the following status.
    /// </summary>
    public Entities.ReportStatus[]? Status { get; set; }

    /// <summary>
    /// get/set - Only include report instances with the following status.
    /// </summary>
    public Entities.NotificationStatus[]? NotificationStatus { get; set; }
    #endregion

    #region Constructors
    public DashboardFilter() { }

    public DashboardFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.NotificationId = filter.GetIntNullValue(nameof(this.NotificationId));
        this.Name = filter.GetStringValue(nameof(this.Name));
        this.Keyword = filter.GetStringValue(nameof(this.Keyword));
        this.OwnerId = filter.GetIntNullValue(nameof(this.OwnerId));
        this.IsPublic = filter.GetBoolNullValue(nameof(this.IsPublic));
        this.IsEnabled = filter.GetBoolNullValue(nameof(this.IsEnabled));
        this.Status = filter.GetEnumArrayValue<Entities.ReportStatus>(nameof(this.Status));
        this.NotificationStatus = filter.GetEnumArrayValue<Entities.NotificationStatus>(nameof(this.NotificationStatus));
        this.StartDate = filter.GetDateTimeNullValue(nameof(this.StartDate));
        this.EndDate = filter.GetDateTimeNullValue(nameof(this.EndDate));
    }
    #endregion
}
