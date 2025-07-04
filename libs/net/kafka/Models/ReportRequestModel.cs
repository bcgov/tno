using System.Text.Json;

namespace TNO.Kafka.Models;

/// <summary>
/// ReportRequestModel class, provides a model for requesting a report be sent.
/// This is a generic model that can be used for sending different types of reports within the solution.
/// </summary>
public class ReportRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - The report destination.
    /// </summary>
    public ReportDestination Destination { get; set; }

    /// <summary>
    /// get/set - Control the type of report generated by this request.
    /// </summary>
    public Entities.ReportType ReportType { get; set; } = Entities.ReportType.Content;

    /// <summary>
    /// get/set - Foreign key to the event schedule that sent this request.
    /// </summary>
    public int? EventScheduleId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report.
    /// This will result in a new report instance being generated.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report instance.
    /// This will result in either resending a report instance or for a custom report.
    /// </summary>
    public long? ReportInstanceId { get; set; }

    /// <summary>
    /// get/set - JSON object with data to be passed to the report template.
    /// </summary>
    public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Foreign key to user who requested the report.
    /// </summary>
    public int? RequestorId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user who is assigned the report.
    /// </summary>
    public int? AssignedId { get; set; }

    /// <summary>
    /// get/set - Email Address that this report will be sent from.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// get/set - Comma separated email addresses that this report will be sent to instead of subscribers.
    /// </summary>
    public string To { get; set; } = "";

    /// <summary>
    /// get/set - Whether to update Razor template cache.
    /// </summary>
    public bool UpdateCache { get; set; }

    /// <summary>
    /// get/set - Whether this request will generate an instance.
    /// This can be used to test as it won't result in a change in the database.
    /// </summary>
    public bool GenerateInstance { get; set; } = true;

    /// <summary>
    /// get/set - Whether the report will be sent to subscribers.
    /// </summary>
    public bool SendToSubscribers { get; set; } = true;

    /// <summary>
    /// get/set - Whether to resend to subscribers even if already sent.
    /// </summary>
    public bool Resend { get; set; } = false;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportRequestModel object.
    /// </summary>
    public ReportRequestModel() { }

    /// <summary>
    /// Creates a new instance of an ReportRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="type"></param>
    /// <param name="reportId"></param>
    /// <param name="data"></param>
    public ReportRequestModel(ReportDestination destination, Entities.ReportType type, int reportId, JsonDocument data)
    {
        this.ReportType = type;
        this.Destination = destination;
        this.ReportId = reportId;
        this.Data = data;
    }

    /// <summary>
    /// Creates a new instance of an ReportRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="type"></param>
    /// <param name="reportId"></param>
    /// <param name="data"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public ReportRequestModel(ReportDestination destination, Entities.ReportType type, int reportId, JsonDocument data, int assignedId, string to = "")
        : this(destination, type, reportId, data)
    {
        this.AssignedId = assignedId;
        this.To = to;
    }

    /// <summary>
    /// Creates a new instance of an ReportRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="type"></param>
    /// <param name="reportId"></param>
    /// <param name="data"></param>
    /// <param name="requestorId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public ReportRequestModel(ReportDestination destination, Entities.ReportType type, int reportId, JsonDocument data, int requestorId, int assignedId, string to = "")
        : this(destination, type, reportId, data, assignedId, to)
    {
        this.RequestorId = requestorId;
    }

    /// <summary>
    /// Creates a new instance of an ReportRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="type"></param>
    /// <param name="reportId"></param>
    /// <param name="reportInstanceId"></param>
    /// <param name="data"></param>
    public ReportRequestModel(ReportDestination destination, Entities.ReportType type, int reportId, long reportInstanceId, JsonDocument data)
        : this(destination, type, reportId, data)
    {
        this.ReportInstanceId = reportInstanceId;
    }

    /// <summary>
    /// Creates a new instance of an ReportRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="type"></param>
    /// <param name="reportId"></param>
    /// <param name="reportInstanceId"></param>
    /// <param name="data"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public ReportRequestModel(ReportDestination destination, Entities.ReportType type, int reportId, long reportInstanceId, JsonDocument data, int assignedId, string to = "")
        : this(destination, type, reportId, reportInstanceId, data)
    {
        this.AssignedId = assignedId;
        this.To = to;
    }

    /// <summary>
    /// Creates a new instance of an ReportRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="type"></param>
    /// <param name="reportId"></param>
    /// <param name="reportInstanceId"></param>
    /// <param name="data"></param>
    /// <param name="requestorId"></param>
    /// <param name="assignedId"></param>
    /// <param name="to"></param>
    public ReportRequestModel(ReportDestination destination, Entities.ReportType type, int reportId, long reportInstanceId, JsonDocument data, int requestorId, int assignedId, string to = "")
        : this(destination, type, reportId, reportInstanceId, data, assignedId, to)
    {
        this.RequestorId = requestorId;
    }
    #endregion
}
