namespace TNO.Kafka.Models;

/// <summary>
/// WorkOrderRequestModel class, a request for the Event Handler to work on a work order. The work
/// order holds the configuration and progress; the message only identifies it, so a work order that
/// does its work a page at a time sends itself another message to continue.
/// </summary>
public class WorkOrderRequestModel : WorkOrderModel
{
    #region Properties
    /// <summary>
    /// get/set - The work order version the message continues from. A message for an older version
    /// is ignored, so only one chain of messages works on a work order.
    /// </summary>
    public long? Version { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderRequestModel object.
    /// </summary>
    public WorkOrderRequestModel() { }

    /// <summary>
    /// Creates a new instance of a WorkOrderRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    public WorkOrderRequestModel(Entities.WorkOrder workOrder) : base(workOrder) { }

    /// <summary>
    /// Creates a new instance of a WorkOrderRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <param name="workType"></param>
    /// <param name="requestor"></param>
    /// <param name="requestedOn"></param>
    public WorkOrderRequestModel(long workOrderId, Entities.WorkOrderType workType, string requestor, DateTime requestedOn)
        : base(workOrderId, workType, requestor, requestedOn) { }
    #endregion
}
