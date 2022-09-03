using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// Content class, provides a way to store generic content items into the database.
/// </summary>
[Table("content")]
public class Content : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - The primary key to uniquely identify content (identity seed).
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("status")]
    public ContentStatus Status { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("workflow_status")]
    public WorkflowStatus WorkflowStatus { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("content_type_id")]
    public int ContentTypeId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public virtual ContentType? ContentType { get; set; }

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
    [Column("series_id")]
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public virtual Series? Series { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("owner_id")]
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public virtual User? Owner { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("data_source_id")]
    public int? DataSourceId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public virtual DataSource? DataSource { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("source")]
    public string Source { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("headline")]
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("uid")]
    public string Uid { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("page")]
    public string Page { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("published_on")]
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("summary")]
    public string Summary { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("transcription")]
    public string Transcription { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    [Column("source_url")]
    public string SourceUrl { get; set; } = "";

    /// <summary>
    /// get/set - The one-to-one relationship to print content (if this content is print content).
    /// </summary>
    public virtual PrintContent? PrintContent { get; set; }

    /// <summary>
    /// get - Collection of logs associated with this content.
    /// </summary>
    public virtual List<ContentLog> Logs { get; } = new List<ContentLog>();

    /// <summary>
    /// get - Collection of categories associated with this content.
    /// </summary>
    public virtual List<Category> Categories { get; } = new List<Category>();

    /// <summary>
    /// get - Collection of categories, the many-to-many relationship.
    /// </summary>
    public virtual List<ContentCategory> CategoriesManyToMany { get; } = new List<ContentCategory>();

    /// <summary>
    /// get - Collection of tone pools associated with this content.
    /// </summary>
    public virtual List<TonePool> TonePools { get; } = new List<TonePool>();

    /// <summary>
    /// get - Collection of tone pools, the many-to-many relationship.
    /// </summary>
    public virtual List<ContentTonePool> TonePoolsManyToMany { get; } = new List<ContentTonePool>();

    /// <summary>
    /// get - Collection of actions associated with this content.
    /// </summary>
    public virtual List<Action> Actions { get; } = new List<Action>();

    /// <summary>
    /// get - Collection of actions, the many-to-many relationship.
    /// </summary>
    public virtual List<ContentAction> ActionsManyToMany { get; } = new List<ContentAction>();

    /// <summary>
    /// get - Collection of labels associated with this content.
    /// </summary>
    public virtual List<ContentLabel> Labels { get; } = new List<ContentLabel>();

    /// <summary>
    /// get - Collection of tags associated with this content.
    /// </summary>
    public virtual List<Tag> Tags { get; } = new List<Tag>();

    /// <summary>
    /// get - Collection of tags, the many-to-many relationship.
    /// </summary>
    public virtual List<ContentTag> TagsManyToMany { get; } = new List<ContentTag>();

    /// <summary>
    /// get - Collection of time tracking for this content.
    /// </summary>
    public virtual List<TimeTracking> TimeTrackings { get; } = new List<TimeTracking>();

    /// <summary>
    /// get - Collection of file references.  While the DB supports multiple file references, presently the intent is only to have a single file for content.
    /// </summary>
    public virtual List<FileReference> FileReferences { get; } = new List<FileReference>();

    /// <summary>
    /// get - Collection of links to related content.
    /// </summary>
    public virtual List<ContentLink> Links { get; } = new List<ContentLink>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Content object.
    /// </summary>
    protected Content() { }

    /// <summary>
    /// Creates a new instance of a Content object, initialises with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="mediaType"></param>
    /// <param name="license"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string source, ContentType contentType, MediaType mediaType, License license, User? owner = null)
    {
        if (String.IsNullOrWhiteSpace(headline)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(headline));
        if (String.IsNullOrWhiteSpace(source)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(source));

        this.Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        this.Headline = headline;
        this.Source = source;
        this.ContentTypeId = contentType?.Id ?? throw new ArgumentNullException(nameof(contentType));
        this.ContentType = contentType;
        this.MediaTypeId = mediaType?.Id ?? throw new ArgumentNullException(nameof(mediaType));
        this.MediaType = mediaType;
        this.LicenseId = license?.Id ?? throw new ArgumentNullException(nameof(license));
        this.License = license;
        this.OwnerId = owner?.Id;
        this.Owner = owner;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initialises with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="source"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="mediaTypeId"></param>
    /// <param name="licenseId"></param>
    /// <param name="ownerId"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string source, int contentTypeId, int mediaTypeId, int licenseId, int? ownerId = null)
    {
        if (String.IsNullOrWhiteSpace(headline)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(headline));
        if (String.IsNullOrWhiteSpace(source)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(source));

        this.Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        this.Headline = headline;
        this.Source = source;
        this.ContentTypeId = contentTypeId;
        this.MediaTypeId = mediaTypeId;
        this.LicenseId = licenseId;
        this.OwnerId = ownerId;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initialises with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="dataSource"></param>
    /// <param name="contentType"></param>
    /// <param name="mediaType"></param>
    /// <param name="license"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, DataSource dataSource, ContentType contentType, MediaType mediaType, License license, User? owner = null) : this(uid, headline, dataSource.Code, contentType, mediaType, license, owner)
    {
        this.DataSourceId = dataSource?.Id ?? throw new ArgumentNullException(nameof(dataSource));
        this.DataSource = dataSource;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initialises with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="dataSource"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="mediaTypeId"></param>
    /// <param name="licenseId"></param>
    /// <param name="ownerId"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, DataSource dataSource, int contentTypeId, int mediaTypeId, int licenseId, int? ownerId = null) : this(uid, headline, dataSource.Code, contentTypeId, mediaTypeId, licenseId, ownerId)
    {
        this.DataSourceId = dataSource?.Id ?? throw new ArgumentNullException(nameof(dataSource));
        this.DataSource = dataSource;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initialises with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="source"></param>
    /// <param name="dataSourceId"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="mediaTypeId"></param>
    /// <param name="licenseId"></param>
    /// <param name="ownerId"></param>
    public Content(string uid, string headline, string source, int? dataSourceId, int contentTypeId, int mediaTypeId, int licenseId, int? ownerId = null) : this(uid, headline, source, contentTypeId, mediaTypeId, licenseId, ownerId)
    {
        this.DataSourceId = dataSourceId;
    }
    #endregion
}
