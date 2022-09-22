using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Source class, provides an entity model to describe different sources of data.
/// </summary>
[Cache("sources", "lookups")]
[Table("source")]
public class Source : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Unique abbreviation to identify the source.
    /// </summary>
    [Column("code")]
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set - Friendly name of the source.
    /// </summary>
    [Column("short_name")]
    public string ShortName { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to license.
    /// </summary>
    [Column("license_id")]
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set - The license.
    /// </summary>
    public virtual License? License { get; set; }

    /// <summary>
    /// get/set - Foreign key to user who will own this content.
    /// </summary>
    [Column("owner_id")]
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The user who owns this source.
    /// </summary>
    public virtual User? Owner { get; set; }

    /// <summary>
    /// get/set - Whether content with this series should automatically be transcribed.
    /// </summary>
    [Column("auto_transcribe")]
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Whether content with this series should not allow transcriptions.
    /// </summary>
    [Column("disable_transcribe")]
    public bool DisableTranscribe { get; set; }

    /// <summary>
    /// get - List of content linked to this source.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get - List of ingests linked to this source.
    /// </summary>
    public virtual List<Ingest> Ingests { get; } = new List<Ingest>();

    /// <summary>
    /// get - List of actions linked to this source.
    /// </summary>
    public virtual List<SourceAction> Actions { get; } = new List<SourceAction>();
    /// <summary>
    /// get - List of actions (many-to-many) linked to this source.
    /// </summary>
    public virtual List<SourceSourceAction> ActionsManyToMany { get; } = new List<SourceSourceAction>();

    /// <summary>
    /// get - List of metrics linked to this source.
    /// </summary>
    public virtual List<Metric> Metrics { get; } = new List<Metric>();

    /// <summary>
    /// get - List of metrics (many-to-many) linked to this source.
    /// </summary>
    public virtual List<SourceMetric> MetricsManyToMany { get; } = new List<SourceMetric>();
    #endregion

    #region Constructors
    protected Source() { }

    public Source(string name, string code, License license) : base(name)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(code)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(code));

        this.Code = code;
        this.LicenseId = license?.Id ?? throw new ArgumentNullException(nameof(license));
        this.License = license;
    }

    public Source(string name, string code, int licenseId) : base(name)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(code)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(code));

        this.Code = code;
        this.LicenseId = licenseId;
    }
    #endregion
}
