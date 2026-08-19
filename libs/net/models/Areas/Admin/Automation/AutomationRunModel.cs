namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationRunModel class, records a single execution of an automation profile.
/// </summary>
public class AutomationRunModel
{
    #region Properties
    /// <summary>
    /// get/set - Run identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Profile id.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// get/set - Run status.
    /// </summary>
    public AutomationRunStatus Status { get; set; }

    /// <summary>
    /// get/set - Trigger source.
    /// </summary>
    public string Trigger { get; set; } = "manual";

    /// <summary>
    /// get/set - Optional run note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// get/set - Start time in UTC.
    /// </summary>
    public DateTime StartedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// get/set - End time in UTC.
    /// </summary>
    public DateTime? CompletedOn { get; set; }

    /// <summary>
    /// get/set - JSON summary of the run outcome (steps, action confirmations/executions, content changes).
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// get/set - Whether this run is a dry run: decisions and changes are computed and logged,
    /// but no content is written and no reports or notifications are sent.
    /// </summary>
    public bool IsDryRun { get; set; }

    /// <summary>
    /// get/set - Optional candidate definition (raw v2 definition JSON) for a comparison run.
    /// </summary>
    public string? CompareDefinition { get; set; }

    /// <summary>
    /// get/set - When the run last posted a response record (its activity heartbeat), if any.
    /// </summary>
    public DateTime? LastResponseOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationRunModel.
    /// </summary>
    public AutomationRunModel() { }

    /// <summary>
    /// Creates a new instance of an AutomationRunModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AutomationRunModel(Entities.AutomationRun entity)
    {
        this.Id = entity.Id;
        this.ProfileId = entity.AutomationProfileId;
        this.Status = (AutomationRunStatus)(int)entity.Status;
        this.Trigger = entity.Trigger;
        this.Note = entity.Note;
        this.StartedOn = entity.StartedOn;
        this.CompletedOn = entity.CompletedOn;
        this.Summary = entity.Summary;
        this.IsDryRun = entity.IsDryRun;
        this.CompareDefinition = entity.CompareDefinition?.RootElement.GetRawText();
        this.LastResponseOn = entity.LastResponseOn;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new AutomationRun entity from this model.
    /// </summary>
    /// <returns></returns>
    public Entities.AutomationRun ToEntity()
    {
        return new Entities.AutomationRun
        {
            Id = this.Id,
            AutomationProfileId = this.ProfileId,
            Status = (Entities.AutomationRunStatus)(int)this.Status,
            Trigger = this.Trigger,
            Note = this.Note,
            StartedOn = this.StartedOn,
            CompletedOn = this.CompletedOn,
            Summary = this.Summary,
            IsDryRun = this.IsDryRun,
            CompareDefinition = !string.IsNullOrWhiteSpace(this.CompareDefinition)
                ? System.Text.Json.JsonDocument.Parse(this.CompareDefinition)
                : null,
        };
    }
    #endregion
}
