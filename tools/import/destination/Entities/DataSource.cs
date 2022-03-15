using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

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

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("data_location_id")]
    public int DataLocationId { get; set; }

    public DataLocation? DataLocation { get; set; }

    [Column("media_type_id")]
    public int MediaTypeId { get; set; }

    public MediaType? MediaType { get; set; }

    [Column("license_id")]
    public int LicenseId { get; set; }

    public License? License { get; set; }

    [Column("topic")]
    public string Topic { get; set; } = "";

    [Column("connection")]
    public string Connection { get; set; } = "{}";

    [Column("last_ran_on")]
    public DateTime? LastRanOn { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    public DataSource? Parent { get; set; }

    public List<Content> Contents { get; } = new List<Content>();
    #endregion

    #region Constructors
    protected DataSource() { }

    public DataSource(string name, string code, DataLocation location, MediaType mediaType, License license, string topic)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(code)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(code));
        if (String.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(topic));

        this.Name = name;
        this.Code = code;
        this.DataLocationId = location?.Id ?? throw new ArgumentNullException(nameof(location));
        this.DataLocation = location;
        this.MediaTypeId = mediaType?.Id ?? throw new ArgumentNullException(nameof(mediaType));
        this.MediaType = mediaType;
        this.LicenseId = license?.Id ?? throw new ArgumentNullException(nameof(license));
        this.License = license;
    }

    public DataSource(string name, string code, int locationId, int mediaTypeId, int licenseId, string topic)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(code)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(code));
        if (String.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(topic));

        this.Name = name;
        this.Code = code;
        this.DataLocationId = locationId;
        this.MediaTypeId = mediaTypeId;
        this.LicenseId = licenseId;
    }
    #endregion
}