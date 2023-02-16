using System.Text.Json;
using TNO.Entities;

namespace TNO.API.Models.SignalR;

/// <summary>
/// WorkOrderMessageModel class, provides a model that represents a work order.
/// </summary>
public class WorkOrderMessageModel : AuditColumnsModel
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an WorkOrderMessageModel.
    /// </summary>
    public WorkOrderMessageModel() { }

    /// <summary>
    /// Creates a new instance of an WorkOrderMessageModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public WorkOrderMessageModel(Entities.WorkOrder entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.WorkType = entity.WorkType;
        this.Status = entity.Status;
        this.RequestorId = entity.RequestorId;
        this.AssignedId = entity.AssignedId;
        this.Description = entity.Description;
        this.Note = entity.Note;
        this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Configuration, options) ?? new Dictionary<string, object>();
    }
    #endregion
}
