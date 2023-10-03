using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.WorkOrder;

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
    /// get/set - The user who requested the work.
    /// </summary>
    public UserModel? Requestor { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user assigned to this work order.
    /// </summary>
    public int? AssignedId { get; set; }

    /// <summary>
    /// get/set - The user assigned the work.
    /// </summary>
    public UserModel? Assigned { get; set; }

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
        this.RequestorId = entity.RequestorId;
        this.AssignedId = entity.AssignedId;
        this.Description = entity.Description;
        this.Note = entity.Note;
        this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Configuration, options) ?? new Dictionary<string, object>();
        this.ContentId = entity.ContentId;

        if (entity.Requestor != null)
            this.Requestor = new UserModel(entity.Requestor);
        if (entity.Assigned != null)
            this.Assigned = new UserModel(entity.Assigned);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Copy values from model to entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.WorkOrder CopyTo(Entities.WorkOrder entity, JsonSerializerOptions options)
    {
        entity.Status = this.Status;
        entity.RequestorId = this.RequestorId;
        entity.AssignedId = this.AssignedId;
        entity.Description = this.Description;
        entity.Note = this.Note;
        entity.Configuration = JsonDocument.Parse(JsonSerializer.Serialize(this.Configuration, options));
        entity.ContentId = this.ContentId;
        entity.Version = this.Version ?? 0;

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
