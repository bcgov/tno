using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// EventItem class, provides an entity to store event records in the database.
/// </summary>
public class EventItem
{
    /// <summary>
    /// get/set.
    /// </summary>
    [Column("RSN")]
    public long RSN { get; set; }

    /// <summary>
    /// get/set - x
    /// </summary>
    [Column("NAME", TypeName = "VARCHAR2")]
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EVENT_TYPE_RSN")]
    public long EventTypeId { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("PROCESS", TypeName = "VARCHAR2")]
    public string? Process { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("CHANNEL", TypeName = "VARCHAR2")]
    public string? Channel { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("SOURCE", TypeName = "VARCHAR2")]
    public string? Source { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("FILE_NAME", TypeName = "VARCHAR2")]
    public string? FileName { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("START_TIME", TypeName = "VARCHAR2")]
    public string? StartTime { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STOP_TIME", TypeName = "VARCHAR2")]
    public string? StopTime { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("LAUNCH_TIME")]
    public string? LaunchTime { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("FREQUENCY", TypeName = "VARCHAR2")]
    public string? Frequency { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("DEFINITION_NAME", TypeName = "VARCHAR2")]
    public string? DefinitionName { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("TITLE", TypeName = "VARCHAR2")]
    public string? Title { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("CAPTURE_COMMAND", TypeName = "VARCHAR2")]
    public string? CaptureCommand { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("CLIP_COMMAND", TypeName = "VARCHAR2")]
    public string? ClipCommand { get; set; }
}
