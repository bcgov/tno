using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.WorkOrder;

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
    /// get/set - The foreign key to the content.
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

    /// <summary>
    /// get/set - The work order configuration.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
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
    /// <param name="options"></param>
    public WorkOrderModel(Entities.WorkOrder entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.WorkType = entity.WorkType;
        this.Status = entity.Status;
        this.ContentId = entity.ContentId;
        this.RequestorId = entity.RequestorId;
        this.AssignedId = entity.AssignedId;
        this.Description = entity.Description;
        this.Note = entity.Note;
        this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Configuration, options) ?? new Dictionary<string, object>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Update specified 'entity' with model property values.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public Entities.WorkOrder CopyTo(Entities.WorkOrder entity, JsonSerializerOptions options)
    {
        entity.WorkType = this.WorkType;
        entity.Status = this.Status;
        entity.ContentId = this.ContentId;
        entity.AssignedId = this.AssignedId;
        entity.RequestorId = this.RequestorId;
        entity.Description = this.Description;
        entity.Note = this.Note;
        entity.Version = this.Version ?? 0;
        entity.Configuration = JsonDocument.Parse(JsonSerializer.Serialize(this.Configuration, options));
        return entity;
    }

    /// <summary>
    /// Copy values from model to entity.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.WorkOrder ToEntity(JsonSerializerOptions options)
    {
        return new Entities.WorkOrder(this.WorkType, this.Description, JsonSerializer.Serialize(this.Configuration, options))
        {
            Id = this.Id,
            Status = this.Status,
            RequestorId = this.RequestorId,
            AssignedId = this.AssignedId,
            Description = this.Description,
            Note = this.Note,
            ContentId = this.ContentId,
            Version = this.Version ?? 0
        };
    }
    #endregion
}
