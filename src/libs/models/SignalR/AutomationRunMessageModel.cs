using Entities = TNO.Entities;

namespace TNO.API.Models.SignalR;

/// <summary>
/// AutomationRunMessageModel class, a lightweight SignalR message describing a change to an
/// automation run (created or status updated). Lets the editor react to scheduled runs in
/// real time instead of requiring a page refresh.
/// </summary>
public class AutomationRunMessageModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key of the automation run.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the automation profile the run belongs to.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// get/set - The current status of the run.
    /// </summary>
    public Entities.AutomationRunStatus Status { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationRunMessageModel object.
    /// </summary>
    public AutomationRunMessageModel() { }

    /// <summary>
    /// Creates a new instance of an AutomationRunMessageModel, initialized with the specified run.
    /// </summary>
    /// <param name="entity"></param>
    public AutomationRunMessageModel(Entities.AutomationRun entity)
    {
        this.Id = entity.Id;
        this.ProfileId = entity.AutomationProfileId;
        this.Status = entity.Status;
    }
    #endregion
}
