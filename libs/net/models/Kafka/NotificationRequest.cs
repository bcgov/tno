namespace TNO.Models.Kafka;

/// <summary>
/// NotificationRequest class, provides a model for requesting a notification be sent.
/// TODO: Change name to NotificationRequestModel for consistent naming convention
/// </summary>
public class NotificationRequest
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the notification template.
    /// </summary>
    public int NotificationTemplateId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the content.
    /// </summary>
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - JSON object with data to be passed to the notification template.
    /// </summary>
    public dynamic Data { get; set; } = new { };

    /// <summary>
    /// get/set - Foreign key to user who requested the notification.
    /// </summary>
    public int? RequestorId { get; set; }

    /// <summary>
    /// get/set - Foriegn key to the user who is assigned the notification.
    /// </summary>
    public int AssignedId { get; set; }

    /// <summary>
    /// get/set - Additional comma separated email addresses that this notification will be sent to.
    /// </summary>
    public string To { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an NotificationRequest object.
    /// </summary>
    public NotificationRequest() { }

    /// <summary>
    /// Creates a new instance of an NotificationRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="notificationTemplateId"></param>
    /// <param name="data"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequest(int notificationTemplateId, object data, int assignedId, string to = "")
    {
        this.NotificationTemplateId = notificationTemplateId;
        this.Data = data;
        this.AssignedId = assignedId;
        this.To = to;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="notificationTemplateId"></param>
    /// <param name="data"></param>
    /// <param name="requestorId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequest(int notificationTemplateId, object data, int requestorId, int assignedId, string to = "")
        : this(notificationTemplateId, data, assignedId, to)
    {
        this.RequestorId = requestorId;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="notificationTemplateId"></param>
    /// <param name="contentId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequest(int notificationTemplateId, long contentId, int assignedId, string to = "")
    {
        this.NotificationTemplateId = notificationTemplateId;
        this.ContentId = contentId;
        this.AssignedId = assignedId;
        this.To = to;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="notificationTemplateId"></param>
    /// <param name="contentId"></param>
    /// <param name="requestorId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequest(int notificationTemplateId, long contentId, int requestorId, int assignedId, string to = "")
        : this(notificationTemplateId, contentId, assignedId, to)
    {
        this.RequestorId = requestorId;
    }
    #endregion
}
