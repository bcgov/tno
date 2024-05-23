using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

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
    /// get/set - Work order configuration.
    /// </summary>
    [Column("configuration")]
    public JsonDocument Configuration { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Foreign key to the content this work order is associated with.
    /// </summary>
    [Column("content_id")]
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - Content work order is associated with.
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
    /// <param name="description"></param>
    /// <param name="configuration"></param>
    public WorkOrder(WorkOrderType type, string description, JsonDocument configuration) : this(type, description)
    {
        this.Configuration = configuration;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="description"></param>
    /// <param name="configuration"></param>
    public WorkOrder(WorkOrderType type, string description, string configuration) : this(type, description, JsonDocument.Parse(configuration))
    {
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="requestor"></param>
    /// <param name="configuration"></param>
    public WorkOrder(WorkOrderType type, User requestor, string description, JsonDocument configuration) : this(type, description, configuration)
    {
        this.RequestorId = requestor.Id;
        this.Requestor = requestor;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="requestor"></param>
    /// <param name="description"></param>
    /// <param name="configuration"></param>
    public WorkOrder(WorkOrderType type, User requestor, string description, string configuration) : this(type, description, configuration)
    {
        this.RequestorId = requestor.Id;
        this.Requestor = requestor;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="requestorId"></param>
    /// <param name="description"></param>
    /// <param name="configuration"></param>
    public WorkOrder(WorkOrderType type, int requestorId, string description, string configuration) : this(type, description, configuration)
    {
        this.RequestorId = requestorId;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="description"></param>
    /// <param name="content"></param>
    public WorkOrder(WorkOrderType type, string description, Content content)
         : this(type, description, content.Id, content.Headline)
    {
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="description"></param>
    /// <param name="contentId"></param>
    /// <param name="headline"></param>
    public WorkOrder(WorkOrderType type, string description, long contentId, string headline)
        : this(type, description, $"{{ \"headline\": \"{headline.Replace("\n", "")}\" }}")
    {
        this.ContentId = contentId;
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="requestor"></param>
    /// <param name="description"></param>
    /// <param name="content"></param>
    /// <param name="configuration"></param>
    public WorkOrder(WorkOrderType type, User? requestor, string description, Content content, JsonDocument? configuration = null)
        : this(type, description, content)
    {
        if (requestor != null)
        {
            this.RequestorId = requestor.Id;
            this.Requestor = requestor;
        }
        this.ContentId = content.Id;
        this.Content = content;
        this.Configuration = configuration ?? JsonDocument.Parse("{}");
    }

    /// <summary>
    /// Creates a new instance of a WorkOrder object, initializes with specified parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="requestorId"></param>
    /// <param name="description"></param>
    /// <param name="contentId"></param>
    /// <param name="headline"></param>
    /// <param name="configuration"></param>
    public WorkOrder(WorkOrderType type, int requestorId, string description, long contentId, string headline, JsonDocument? configuration = null)
        : this(type, description, contentId, headline)
    {
        this.RequestorId = requestorId;
        this.Configuration = configuration ?? JsonDocument.Parse("{}");
    }
    #endregion
}
