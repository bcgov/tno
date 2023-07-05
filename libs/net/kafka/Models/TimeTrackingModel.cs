namespace TNO.Kafka.Models;

/// <summary>
/// TimeTracking class, provides a model that represents a TimeTracking updates to a news item.
/// </summary>
public class TimeTrackingModel
{
    #region Properties
    /// <summary>
    /// get/set - What kind of activity is being tracked Created/Updated.
    /// </summary>
    public string Activity { get; set; } = "";

    /// <summary>
    /// get/set - The number of minutes spent.
    /// </summary>
    public float Effort { get; set; }

    /// <summary>
    /// get/set - Foreign key to user.
    /// </summary>
    public int UserId { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TimeTracking object.
    /// </summary>
    public TimeTrackingModel() { }

    /// <summary>
    /// Creates a new instance of a TimeTracking object, initializes with specified parameters.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public TimeTrackingModel(string activity, float effort)
    {
        this.Activity = activity ?? "";
        this.Effort = effort;
    }
    #endregion
}
