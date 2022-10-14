using Microsoft.Extensions.Primitives;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.DAL.Models;

/// <summary>
/// WorkOrderFilter class, provides a model for searching work order.
/// </summary>
public class WorkOrderFilter : PageFilter
{
    #region Properties
    /// <summary>
    /// get/set - Only include work order with this status.
    /// </summary>
    public WorkOrderStatus? Status { get; set; }

    /// <summary>
    /// get/set - Only include work order with this work type.
    /// </summary>
    public WorkOrderType? WorkType { get; set; }

    /// <summary>
    /// get/set - Only include work order with this content id.
    /// </summary>
    public long? ContentId { get; set; }

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

    /// <summary>
    /// get/set - Sort the work order in the specified order.
    /// </summary>
    public string[] Sort { get; set; } = Array.Empty<string>();
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

        this.Status = filter.GetEnumNullValue<WorkOrderStatus>(nameof(this.Status));
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

        this.Sort = filter.GetStringArrayValue(nameof(this.Sort));
    }
    #endregion
}
