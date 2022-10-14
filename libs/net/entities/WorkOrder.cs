using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// WorkOrder class, provides an entity model to manage work order requests.
/// </summary>
[Table("work_order")]
public class WorkOrder : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key, identity seed.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - The type of work order request.
    /// </summary>
    [Column("work_type")]
    public WorkOrderType WorkType { get; set; }

    /// <summary>
    /// get/set - The status of the work order.
    /// </summary>
    [Column("status")]
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Submitted;

    /// <summary>
    /// get/set - Foreign key to user who requested the work order.
    /// </summary>
    [Column("requestor_id")]
    public int? RequestorId { get; set; }

    /// <summary>
    /// get/set - The user who requested the work order.
    /// </summary>
    public User? Requestor { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user assigned the work order.
    /// </summary>
    [Column("assigned_id")]
    public int? AssignedId { get; set; }

    /// <summary>
    /// get/set - The user assigned the work order.
    /// </summary>
    public User? Assigned { get; set; }

    /// <summary>
    /// get/set - A description of the work order.
    /// </summary>
    [Column("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Notes related to the work order.
    /// </summary>
    [Column("note")]
    public string Note { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to the content associated with this work order.
    /// </summary>
    [Column("content_id")]
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - The content associated with this work order.
    /// </summary>
    public Content? Content { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrder object.
    /// </summary>
    protected WorkOrder() { }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="description"></param>
    public WorkOrder(WorkOrderType type, string description) : base()
    {
        this.WorkType = type;
        this.Description = description;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="requestor"></param>
    /// <param name="description"></param>
    public WorkOrder(WorkOrderType type, User requestor, string description) : base()
    {
        this.WorkType = type;
        this.RequestorId = requestor?.Id;
        this.Requestor = requestor;
        this.Description = description;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="requestorId"></param>
    /// <param name="description"></param>
    public WorkOrder(WorkOrderType type, int requestorId, string description) : base()
    {
        this.WorkType = type;
        this.RequestorId = requestorId;
        this.Description = description;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="content"></param>
    /// <param name="description"></param>
    public WorkOrder(WorkOrderType type, Content content, string description) : base()
    {
        this.WorkType = type;
        this.ContentId = content?.Id;
        this.Content = content;
        this.Description = description;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="contentId"></param>
    /// <param name="description"></param>
    public WorkOrder(WorkOrderType type, long contentId, string description) : base()
    {
        this.WorkType = type;
        this.ContentId = contentId;
        this.Description = description;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="content"></param>
    /// <param name="requestor"></param>
    /// <param name="description"></param>
    public WorkOrder(WorkOrderType type, Content content, User requestor, string description) : base()
    {
        this.WorkType = type;
        this.ContentId = content?.Id;
        this.Content = content;
        this.RequestorId = requestor?.Id;
        this.Requestor = requestor;
        this.Description = description;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="contentId"></param>
    /// <param name="requestorId"></param>
    /// <param name="description"></param>
    public WorkOrder(WorkOrderType type, long contentId, int requestorId, string description) : base()
    {
        this.WorkType = type;
        this.ContentId = contentId;
        this.RequestorId = requestorId;
        this.Description = description;
    }
    #endregion
}
