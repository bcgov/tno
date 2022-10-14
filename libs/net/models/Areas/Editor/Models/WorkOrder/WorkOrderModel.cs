using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.WorkOrder;

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
    /// get/set - Foreign key to the content to work on.
    /// </summary>
    public long? ContentId { get; set; }

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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an WorkOrderModel.
    /// </summary>
    public WorkOrderModel() { }

    /// <summary>
    /// Creates a new instance of an WorkOrderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public WorkOrderModel(Entities.WorkOrder entity) : base(entity)
    {
        this.Id = entity.Id;
        this.WorkType = entity.WorkType;
        this.Status = entity.Status;
        this.ContentId = entity.ContentId;
        this.RequestorId = entity.RequestorId;
        this.AssignedId = entity.AssignedId;
        this.Description = entity.Description;
        this.Note = entity.Note;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.WorkOrder(WorkOrderModel model)
    {
        return new Entities.WorkOrder(model.WorkType, model.Description)
        {
            Id = model.Id,
            Status = model.Status,
            ContentId = model.ContentId,
            AssignedId = model.AssignedId,
            RequestorId = model.RequestorId,
            Note = model.Note,
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
