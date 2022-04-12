using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("data_source")]
public class DataSource : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("code")]
    public string Code { get; set; } = "";

    [Column("short_name")]
    public string ShortName { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("data_location_id")]
    public int DataLocationId { get; set; }

    public virtual DataLocation? DataLocation { get; set; }

    [Column("media_type_id")]
    public int MediaTypeId { get; set; }

    public virtual MediaType? MediaType { get; set; }

    [Column("license_id")]
    public int LicenseId { get; set; }

    public virtual License? License { get; set; }

    [Column("topic")]
    public string Topic { get; set; } = "";

    [Column("connection")]
    public string Connection { get; set; } = "{}";

    [Column("last_ran_on")]
    public DateTime? LastRanOn { get; set; }

    [Column("retry_limit")]
    public int RetryLimit { get; set; }

    [Column("failed_attempts")]
    public int FailedAttempts { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    public virtual DataSource? Parent { get; set; }

    public virtual List<Content> Contents { get; } = new List<Content>();

    public virtual List<SourceAction> Actions { get; } = new List<SourceAction>();

    public virtual List<DataSourceAction> ActionsManyToMany { get; } = new List<DataSourceAction>();

    public virtual List<SourceMetric> Metrics { get; } = new List<SourceMetric>();

    public virtual List<DataSourceMetric> MetricsManyToMany { get; } = new List<DataSourceMetric>();

    public virtual List<Schedule> Schedules { get; } = new List<Schedule>();

    public virtual List<DataSourceSchedule> SchedulesManyToMany { get; } = new List<DataSourceSchedule>();
    #endregion

    #region Constructors
    protected DataSource() { }

    public DataSource(string name, string code, DataLocation location, MediaType mediaType, License license, string topic)
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
        this.Topic = topic ?? throw new ArgumentNullException(nameof(topic));
    }

    public DataSource(string name, string code, int locationId, int mediaTypeId, int licenseId, string topic)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(code)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(code));

        this.Name = name;
        this.Code = code;
        this.DataLocationId = locationId;
        this.MediaTypeId = mediaTypeId;
        this.LicenseId = licenseId;
        this.Topic = topic ?? throw new ArgumentNullException(nameof(topic));
    }
    #endregion
}
