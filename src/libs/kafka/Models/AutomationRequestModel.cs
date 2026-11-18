namespace TNO.Kafka.Models;

/// <summary>
/// AutomationRequestModel class, provides a Kafka message model to request an automation run
/// be executed by the automation service.
/// </summary>
public class AutomationRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - The automation run to execute.
    /// </summary>
    public long RunId { get; set; }

    /// <summary>
    /// get/set - The automation profile the run belongs to.
    /// </summary>
    public int ProfileId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationRequestModel object.
    /// </summary>
    public AutomationRequestModel() { }

    /// <summary>
    /// Creates a new instance of an AutomationRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="runId"></param>
    /// <param name="profileId"></param>
    public AutomationRequestModel(long runId, int profileId)
    {
        this.RunId = runId;
        this.ProfileId = profileId;
    }
    #endregion
}
