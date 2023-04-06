namespace TNO.Kafka.Models;

/// <summary>
/// NotificationRequestModel class, provides a model for requesting a notification be sent.
/// This is a generic model that can be used for sending different types of notifications within the solution.
/// </summary>
public class NotificationRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - The notification destination.
    /// </summary>
    public NotificationDestination Destination { get; set; }

    /// <summary>
    /// get/set - Foreign key to the notification.
    /// </summary>
    public int? NotificationId { get; set; }

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
    /// get/set - Foreign key to the user who is assigned the notification.
    /// </summary>
    public int? AssignedId { get; set; }

    /// <summary>
    /// get/set - Additional comma separated email addresses that this notification will be sent to.
    /// </summary>
    public string To { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object.
    /// </summary>
    public NotificationRequestModel() { }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="data"></param>
    public NotificationRequestModel(NotificationDestination destination, object data)
    {
        this.Destination = destination;
        this.Data = data;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="data"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequestModel(NotificationDestination destination, object data, int assignedId, string to = "")
        : this(destination, data)
    {
        this.AssignedId = assignedId;
        this.To = to;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="notificationId"></param>
    /// <param name="data"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequestModel(NotificationDestination destination, int notificationId, object data, int assignedId, string to = "")
    {
        this.Destination = destination;
        this.NotificationId = notificationId;
        this.Data = data;
        this.AssignedId = assignedId;
        this.To = to;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="data"></param>
    /// <param name="requestorId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequestModel(NotificationDestination destination, object data, int requestorId, int assignedId, string to = "")
        : this(destination, data, assignedId, to)
    {
        this.RequestorId = requestorId;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="notificationId"></param>
    /// <param name="data"></param>
    /// <param name="requestorId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequestModel(NotificationDestination destination, int notificationId, object data, int requestorId, int assignedId, string to = "")
        : this(destination, notificationId, data, assignedId, to)
    {
        this.RequestorId = requestorId;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="contentId"></param>
    public NotificationRequestModel(NotificationDestination destination, long contentId)
    {
        this.Destination = destination;
        this.ContentId = contentId;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="contentId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequestModel(NotificationDestination destination, long contentId, int assignedId, string to = "")
        : this(destination, contentId)
    {
        this.AssignedId = assignedId;
        this.To = to;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequestModel(NotificationDestination destination, int notificationId, long contentId, int assignedId, string to = "")
    {
        this.Destination = destination;
        this.NotificationId = notificationId;
        this.ContentId = contentId;
        this.AssignedId = assignedId;
        this.To = to;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="contentId"></param>
    /// <param name="requestorId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequestModel(NotificationDestination destination, long contentId, int requestorId, int assignedId, string to = "")
        : this(destination, contentId, assignedId, to)
    {
        this.RequestorId = requestorId;
    }

    /// <summary>
    /// Creates a new instance of an NotificationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    /// <param name="requestorId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public NotificationRequestModel(NotificationDestination destination, int notificationId, long contentId, int requestorId, int assignedId, string to = "")
        : this(destination, notificationId, contentId, assignedId, to)
    {
        this.RequestorId = requestorId;
    }
    #endregion
}
