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
    [Column("NAME")]
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("EVENT_TYPE_RSN")]
    public long EventTypeId { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("PROCESS")]
    public string? Process { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("CHANNEL")]
    public string? Channel { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("SOURCE")]
    public string? Source { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("FILE_NAME")]
    public string? FileName { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("START_TIME")]
    public string? StartTime { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("STOP_TIME")]
    public string? StopTime { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("LAUNCH_TIME")]
    public string? LaunchTime { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("FREQUENCY")]
    public string? Frequency { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("DEFINITION_NAME")]
    public string? DefinitionName { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("TITLE")]
    public string? Title { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("CAPTURE_COMMAND")]
    public string? CaptureCommand { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    [Column("CLIP_COMMAND")]
    public string? ClipCommand { get; set; }
}
