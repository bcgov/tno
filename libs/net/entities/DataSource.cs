using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// DataSource class, provides a way to desribe and store a source of data.
/// Includes information on how to connect to the data source and how services are run and scheduled.
/// </summary>
[Cache("data_sources", "lookups")]
[Table("data_source")]
public class DataSource : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("code")]
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("short_name")]
    public string ShortName { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - Foreign key to the data location.
    /// </summary>
    [Column("data_location_id")]
    public int DataLocationId { get; set; }

    /// <summary>
    /// get/set - The data location where files will be uploaded.
    /// This is not the source location, but rather the location files will be uploaded to when ingested.
    /// </summary>
    public virtual DataLocation? DataLocation { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("media_type_id")]
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public virtual MediaType? MediaType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("license_id")]
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public virtual License? License { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("schedule_type")]
    public DataSourceScheduleType ScheduleType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("topic")]
    public string Topic { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("connection")]
    public string Connection { get; set; } = "{}";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("last_ran_on")]
    public DateTime? LastRanOn { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("retry_limit")]
    public int RetryLimit { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("failed_attempts")]
    public int FailedAttempts { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("parent_id")]
    public int? ParentId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public virtual DataSource? Parent { get; set; }

    /// <summary>
    /// get -
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get -
    /// </summary>
    public virtual List<SourceAction> Actions { get; } = new List<SourceAction>();

    /// <summary>
    /// get -
    /// </summary>
    public virtual List<DataSourceAction> ActionsManyToMany { get; } = new List<DataSourceAction>();

    /// <summary>
    /// get -
    /// </summary>
    public virtual List<SourceMetric> Metrics { get; } = new List<SourceMetric>();

    /// <summary>
    /// get -
    /// </summary>
    public virtual List<DataSourceMetric> MetricsManyToMany { get; } = new List<DataSourceMetric>();

    /// <summary>
    /// get -
    /// </summary>
    public virtual List<Schedule> Schedules { get; } = new List<Schedule>();

    /// <summary>
    /// get -
    /// </summary>
    public virtual List<DataSourceSchedule> SchedulesManyToMany { get; } = new List<DataSourceSchedule>();
    #endregion

    #region Constructors
    protected DataSource() { }

    public DataSource(string name, string code, DataLocation location, MediaType mediaType, License license, DataSourceScheduleType scheduleType, string topic)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(code)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(code));

        this.Name = name;
        this.Code = code;
        this.DataLocationId = location?.Id ?? throw new ArgumentNullException(nameof(location));
        this.DataLocation = location;
        this.MediaTypeId = mediaType?.Id ?? throw new ArgumentNullException(nameof(mediaType));
        this.MediaType = mediaType;
        this.LicenseId = license?.Id ?? throw new ArgumentNullException(nameof(license));
        this.License = license;
        this.ScheduleType = scheduleType;
        this.Topic = topic ?? throw new ArgumentNullException(nameof(topic));
    }

    public DataSource(string name, string code, int locationId, int mediaTypeId, int licenseId, DataSourceScheduleType scheduleType, string topic)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(code)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(code));

        this.Name = name;
        this.Code = code;
        this.DataLocationId = locationId;
        this.MediaTypeId = mediaTypeId;
        this.LicenseId = licenseId;
        this.ScheduleType = scheduleType;
        this.Topic = topic ?? throw new ArgumentNullException(nameof(topic));
    }
    #endregion
}
