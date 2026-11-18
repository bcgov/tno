using TNO.Entities;

namespace TNO.Kafka.Models;

public abstract class WorkOrderModel
{
    #region Properties
    public long WorkOrderId { get; set; }
    public WorkOrderType WorkType { get; set; }
    public int? RequestorId { get; set; }
    public string Requestor { get; set; } = "";
    public DateTime RequestedOn { get; set; }
    public int? AssignedToId { get; set; }
    public string AssignedTo { get; set; } = "";
    #endregion

    #region Constructors
    public WorkOrderModel() { }

    public WorkOrderModel(WorkOrderType workType)
    {
        this.WorkType = workType;
        this.RequestedOn = DateTime.UtcNow;
    }

    public WorkOrderModel(WorkOrder workOrder)
    {
        this.WorkOrderId = workOrder.Id;
        this.WorkType = workOrder.WorkType;
        this.RequestorId = workOrder.RequestorId;
        this.Requestor = workOrder.Requestor?.DisplayName ?? "";
        this.RequestedOn = workOrder.CreatedOn;
        this.AssignedToId = workOrder.AssignedId;
        this.AssignedTo = workOrder.Assigned?.DisplayName ?? "";
    }

    public WorkOrderModel(long workOrderId, WorkOrderType workType, string requestor, DateTime requestedOn)
    {
        this.WorkOrderId = workOrderId;
        this.WorkType = workType;
        this.Requestor = requestor;
        this.RequestedOn = requestedOn;
    }

    public WorkOrderModel(long workOrderId, WorkOrderType workType, int? requestorId, string requestor, DateTime requestedOn) : this(workOrderId, workType, requestor, requestedOn)
    {
        this.RequestorId = requestorId;
    }
    #endregion
}
