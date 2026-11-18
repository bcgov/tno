using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Subscriber.Models.WorkOrder;

/// <summary>
/// WorkOrderModel class, provides a model that represents a work order.
/// </summary>
public class WorkOrderModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - The type of work order.
    /// </summary>
    public WorkOrderType WorkType { get; set; }

    /// <summary>
    /// get/set - The status of the work order.
    /// </summary>
    public WorkOrderStatus Status { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user requested this work order.
    /// </summary>
    public int? RequestorId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user assigned to this work order.
    /// </summary>
    public int? AssignedId { get; set; }

    /// <summary>
    /// get/set - Description of the work order.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Notes about the work order.
    /// </summary>
    public string Note { get; set; } = "";

    /// <summary>
    /// get/set - The work order configuration.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set - Foreign key to content.
    /// </summary>
    public long? ContentId { get; set; }

    /// <summary>
    /// get - Collection of users who want to be notified about this content.
    /// </summary>
    public IEnumerable<UserContentNotificationModel> UserNotifications { get; } = Array.Empty<UserContentNotificationModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an WorkOrderModel.
    /// </summary>
    public WorkOrderModel() { }

    /// <summary>
    /// Creates a new instance of an WorkOrderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="content"></param>
    /// <param name="options"></param>
    public WorkOrderModel(Entities.WorkOrder workOrder, Entities.Content? content, JsonSerializerOptions options) : base(workOrder)
    {
        this.Id = workOrder.Id;
        this.WorkType = workOrder.WorkType;
        this.Status = workOrder.Status;
        this.RequestorId = workOrder.RequestorId;
        this.AssignedId = workOrder.AssignedId;
        this.Description = workOrder.Description;
        this.Note = workOrder.Note;
        this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(workOrder.Configuration, options) ?? new Dictionary<string, object>();
        this.ContentId = workOrder.ContentId;
        this.UserNotifications = content?.UserNotifications.Select(un => new UserContentNotificationModel(un)) ?? Array.Empty<UserContentNotificationModel>();
    }
    #endregion
}
