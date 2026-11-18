namespace TNO.Kafka.Models;

/// <summary>
/// EventScheduleRequestModel class, provides a model for requesting an event.
/// </summary>
public class EventScheduleRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the event schedule that sent this request.
    /// </summary>
    public int EventScheduleId { get; set; }

    /// <summary>
    /// get/set - JSON object with data to be passed to the notification template.
    /// </summary>
    public dynamic Data { get; set; } = new { };
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an EventScheduleRequestModel object.
    /// </summary>
    public EventScheduleRequestModel() { }

    /// <summary>
    /// Creates a new instance of an EventScheduleRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="eventScheduleId"></param>
    /// <param name="data"></param>
    public EventScheduleRequestModel(int eventScheduleId, object data)
    {
        this.EventScheduleId = eventScheduleId;
        this.Data = data;
    }

    /// <summary>
    /// Creates a new instance of an EventScheduleRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="eventSchedule"></param>
    /// <param name="data"></param>
    public EventScheduleRequestModel(Entities.EventSchedule eventSchedule, object data)
    {
        this.EventScheduleId = eventSchedule.Id;
        this.Data = data;
    }

    /// <summary>
    /// Creates a new instance of an EventScheduleRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="eventSchedule"></param>
    /// <param name="data"></param>
    public EventScheduleRequestModel(API.Areas.Services.Models.EventSchedule.EventScheduleModel eventSchedule, object data)
    {
        this.EventScheduleId = eventSchedule.Id;
        this.Data = data;
    }
    #endregion
}
