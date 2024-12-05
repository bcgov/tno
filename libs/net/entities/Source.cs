using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
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
    /// get/set - Common call, or friendly name of the source.
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
    /// get/set - Foreign key to media that content will be defaulted to.
    /// </summary>
    [Column("media_type_id")]
    public int? MediaTypeId { get; set; }

    /// <summary>
    /// get - Collection of media types - used in search mapping.
    /// </summary>
    public virtual List<MediaType> MediaTypeSearchMappings { get; } = new List<MediaType>();

    /// <summary>
    /// get - Collection of media types used in search mapping, the many-to-many relationship.
    /// </summary>
    public virtual List<SourceMediaTypeSearchMapping> MediaTypeSearchMappingsManyToMany { get; } = new List<SourceMediaTypeSearchMapping>();

    /// <summary>
    /// get/set - The default media type designation.
    /// </summary>
    public virtual MediaType? MediaType { get; set; }

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
    /// get/set - Whether to show topics on the content form.
    /// </summary>
    [Column("use_in_topics")]
    public bool UseInTopics { get; set; }

    /// <summary>
    /// get/set - Configuration settings.
    /// </summary>
    [Column("configuration")]
    public JsonDocument Configuration { get; set; } = JsonDocument.Parse("{}");
    
    /// <summary>
    /// get/set - is CBRA source or not.
    /// </summary>
    [Column("is_cbra_source")]
    public bool IsCBRASource { get; set; }

    /// <summary>
    /// get - List of content linked to this source.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get - List of ingests linked to this source.
    /// </summary>
    public virtual List<Ingest> Ingests { get; } = new List<Ingest>();

    /// <summary>
    /// get - List of series linked to this source.
    /// </summary>
    public virtual List<Series> Series { get; } = new List<Series>();

    /// <summary>
    /// get - List of contributors linked to this source.
    /// </summary>
    public virtual List<Contributor> Contributors { get; } = new List<Contributor>();

    /// <summary>
    /// get - List of topic score rules linked to this source.
    /// </summary>
    public virtual List<TopicScoreRule> ScoreRules { get; } = new List<TopicScoreRule>();

    /// <summary>
    /// get - List of metrics linked to this source.
    /// </summary>
    public virtual List<Metric> Metrics { get; } = new List<Metric>();

    /// <summary>
    /// get - List of metrics (many-to-many) linked to this source.
    /// </summary>
    public virtual List<SourceMetric> MetricsManyToMany { get; } = new List<SourceMetric>();

    /// <summary>
    /// get - List of earned media formula configuration for this source.
    /// </summary>
    public virtual List<EarnedMedia> EarnedMedia { get; } = new List<EarnedMedia>();

    /// <summary>
    /// get - List of users linked to this source.
    /// </summary>
    public virtual List<User> Users { get; } = new List<User>();

    /// <summary>
    /// get - List of users (many-to-many) linked to this source.
    /// </summary>
    public virtual List<UserSource> UsersManyToMany { get; } = new List<UserSource>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Source object.
    /// </summary>
    protected Source() { }

    /// <summary>
    /// Creates a new instance of a Source object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="code"></param>
    /// <param name="licenseId"></param>
    /// <exception cref="ArgumentException"></exception>
    public Source(string name, string code, int licenseId) : base(name)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(code)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(code));

        this.Code = code;
        this.LicenseId = licenseId;
    }

    /// <summary>
    /// Creates a new instance of a Source object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="code"></param>
    /// <param name="license"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Source(string name, string code, License license) : base(name)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(code)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(code));

        this.Code = code;
        this.LicenseId = license?.Id ?? throw new ArgumentNullException(nameof(license));
        this.License = license;
    }
    #endregion
}
