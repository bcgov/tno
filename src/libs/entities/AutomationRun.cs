using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AutomationRun class, provides a DB model to record a single execution of an automation profile.
/// Runs are retained for a configured number of days and then pruned.
/// </summary>
[Table("automation_run")]
public class AutomationRun : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the profile that was run.
    /// </summary>
    [Column("automation_profile_id")]
    public int AutomationProfileId { get; set; }

    /// <summary>
    /// get/set - The profile that was run.
    /// </summary>
    public virtual AutomationProfile? AutomationProfile { get; set; }

    /// <summary>
    /// get/set - The lifecycle status of the run.
    /// </summary>
    [Column("status")]
    public AutomationRunStatus Status { get; set; }

    /// <summary>
    /// get/set - The trigger source of the run ('manual', 'scheduled').
    /// </summary>
    [Column("trigger")]
    public string Trigger { get; set; } = "manual";

    /// <summary>
    /// get/set - An optional note describing the run.
    /// </summary>
    [Column("note")]
    public string? Note { get; set; }

    /// <summary>
    /// get/set - When the run started (UTC).
    /// </summary>
    [Column("started_on")]
    public DateTime StartedOn { get; set; }

    /// <summary>
    /// get/set - When the run completed (UTC).
    /// </summary>
    [Column("completed_on")]
    public DateTime? CompletedOn { get; set; }

    /// <summary>
    /// get/set - JSON summary of the run outcome (steps and content changes).
    /// </summary>
    [Column("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// get/set - Whether this run is a dry run: every decision and change is computed and logged,
    /// but no content is written and no reports or notifications are sent.
    /// </summary>
    [Column("is_dry_run")]
    public bool IsDryRun { get; set; }

    /// <summary>
    /// get/set - Optional candidate definition for a comparison run. When present the v2 engine
    /// executes both the profile's definition (variant 'A') and this candidate (variant 'B') as
    /// dry runs over the same trigger and records the differences in the summary and log.
    /// </summary>
    [Column("compare_definition")]
    public System.Text.Json.JsonDocument? CompareDefinition { get; set; }

    /// <summary>
    /// get/set - The engine decision log entries captured during the run (v2 engine; many-to-one).
    /// </summary>
    public virtual ICollection<AutomationRunLog> Logs { get; set; } = new List<AutomationRunLog>();

    /// <summary>
    /// get/set - When the most recent response record was captured for this run (not persisted;
    /// populated by queries). Used as the run's activity heartbeat to detect abandoned runs.
    /// </summary>
    [NotMapped]
    public DateTime? LastResponseOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationRun object.
    /// </summary>
    public AutomationRun() { }

    /// <summary>
    /// Creates a new instance of an AutomationRun object, initializes with specified parameters.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="trigger"></param>
    public AutomationRun(int profileId, string trigger)
    {
        this.AutomationProfileId = profileId;
        this.Trigger = trigger;
    }
    #endregion
}
