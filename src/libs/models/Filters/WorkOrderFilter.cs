using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.Models.Filters;

/// <summary>
/// WorkOrderFilter class, provides a model for searching work order.
/// </summary>
public class WorkOrderFilter : PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - Keywords to search for.
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// get/set - Whether the transcript has been approved.
    /// </summary>
    public bool? IsApproved { get; set; }

    /// <summary>
    /// get/set - Only include work order with this status.
    /// </summary>
    public WorkOrderStatus[]? Status { get; set; }

    /// <summary>
    /// get/set - Only include work order with this work type.
    /// </summary>
    public WorkOrderType? WorkType { get; set; }

    /// <summary>
    /// get/set - Only include work order with this content id.
    /// </summary>
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - An array of mediat types to filter on.
    /// </summary>
    public int[] MediaTypeIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - An array of serries/show/program to filter on.
    /// </summary>
    public int[] SeriesIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - An array of sources to filter on.
    /// </summary>
    public int[] SourceIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - Only include work order requested by this user.
    /// </summary>
    public int? RequestorId { get; set; }

    /// <summary>
    /// get/set - Only include work order assigned to this user.
    /// </summary>
    public int? AssignedId { get; set; }

    /// <summary>
    /// get/set - Only include work order created on this date.
    /// </summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// get/set - Only include work order created on or after this date.
    /// </summary>
    public DateTime? CreatedStartOn { get; set; }

    /// <summary>
    /// get/set - Only included work order created on or before this date.
    /// </summary>
    public DateTime? CreatedEndOn { get; set; }

    /// <summary>
    /// get/set - Only include work order updated on this date.
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// get/set - Only include work order updated on or after this date.
    /// </summary>
    public DateTime? UpdatedStartOn { get; set; }

    /// <summary>
    /// get/set - Only include work order updated on or before this date.
    /// </summary>
    public DateTime? UpdatedEndOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderFilter object.
    /// </summary>
    public WorkOrderFilter() { }

    /// <summary>
    /// Creates a new instance of a WorkOrderFilter object, initializes with specified parameters.
    /// </summary>
    /// <param name="queryParams"></param>
    public WorkOrderFilter(Dictionary<string, StringValues> queryParams) : base(queryParams)
    {
        var filter = new Dictionary<string, StringValues>(queryParams, StringComparer.OrdinalIgnoreCase);

        this.Keywords = filter.GetStringValue(nameof(this.Keywords));

        this.IsApproved = filter.GetBoolNullValue(nameof(this.IsApproved));
        this.Status = filter.GetEnumArrayValue<WorkOrderStatus>(nameof(this.Status));
        this.WorkType = filter.GetEnumNullValue<WorkOrderType>(nameof(this.WorkType));

        this.RequestorId = filter.GetIntNullValue(nameof(this.RequestorId));
        this.AssignedId = filter.GetIntNullValue(nameof(this.AssignedId));

        this.ContentId = filter.GetLongNullValue(nameof(this.ContentId));

        this.CreatedOn = filter.GetDateTimeNullValue(nameof(this.CreatedOn));
        this.CreatedStartOn = filter.GetDateTimeNullValue(nameof(this.CreatedStartOn));
        this.CreatedEndOn = filter.GetDateTimeNullValue(nameof(this.CreatedEndOn));
        this.UpdatedOn = filter.GetDateTimeNullValue(nameof(this.UpdatedOn));
        this.UpdatedStartOn = filter.GetDateTimeNullValue(nameof(this.UpdatedStartOn));
        this.UpdatedEndOn = filter.GetDateTimeNullValue(nameof(this.UpdatedEndOn));

        this.MediaTypeIds = filter.GetIntArrayValue(nameof(this.MediaTypeIds));
        this.SeriesIds = filter.GetIntArrayValue(nameof(this.SeriesIds));
        this.SourceIds = filter.GetIntArrayValue(nameof(this.SourceIds));
    }
    #endregion
}
