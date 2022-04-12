using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content")]
public class Content : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("status")]
    public ContentStatus Status { get; set; }

    [Column("workflow_status")]
    public WorkflowStatus WorkflowStatus { get; set; }

    [Column("content_type_id")]
    public int ContentTypeId { get; set; }

    public virtual ContentType? ContentType { get; set; }

    [Column("media_type_id")]
    public int MediaTypeId { get; set; }

    public virtual MediaType? MediaType { get; set; }

    [Column("license_id")]
    public int LicenseId { get; set; }

    public virtual License? License { get; set; }

    [Column("series_id")]
    public int? SeriesId { get; set; }

    public virtual Series? Series { get; set; }

    [Column("owner_id")]
    public int OwnerId { get; set; }

    public virtual User? Owner { get; set; }

    [Column("data_source_id")]
    public int? DataSourceId { get; set; }

    public virtual DataSource? DataSource { get; set; }

    [Column("source")]
    public string Source { get; set; } = "";

    [Column("headline")]
    public string Headline { get; set; } = "";

    [Column("uid")]
    public string Uid { get; set; } = "";

    [Column("page")]
    public string Page { get; set; } = "";

    [Column("published_on")]
    public DateTime? PublishedOn { get; set; }

    [Column("summary")]
    public string Summary { get; set; } = "";

    [Column("transcription")]
    public string Transcription { get; set; } = "";

    [Column("source_url")]
    public string SourceUrl { get; set; } = "";

    public virtual PrintContent? PrintContent { get; set; }

    public virtual List<ContentLog> Logs { get; } = new List<ContentLog>();

    public virtual List<Category> Categories { get; } = new List<Category>();

    public virtual List<ContentCategory> CategoriesManyToMany { get; } = new List<ContentCategory>();

    public virtual List<TonePool> TonePools { get; } = new List<TonePool>();

    public virtual List<ContentTonePool> TonePoolsManyToMany { get; } = new List<ContentTonePool>();

    public virtual List<Action> Actions { get; } = new List<Action>();

    public virtual List<ContentAction> ActionsManyToMany { get; } = new List<ContentAction>();

    public virtual List<Tag> Tags { get; } = new List<Tag>();

    public virtual List<ContentTag> TagsManyToMany { get; } = new List<ContentTag>();

    public virtual List<TimeTracking> TimeTrackings { get; } = new List<TimeTracking>();

    public virtual List<FileReference> FileReferences { get; } = new List<FileReference>();

    public virtual List<ContentLink> Links { get; } = new List<ContentLink>();
    #endregion

    #region Constructors
    protected Content() { }

    public Content(string uid, string headline, string source, ContentType contentType, MediaType mediaType, License license, User owner)
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
        this.OwnerId = owner?.Id ?? throw new ArgumentNullException(nameof(owner));
        this.Owner = owner;
    }

    public Content(string uid, string headline, string source, int contentTypeId, int mediaTypeId, int licenseId, int ownerId)
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

    public Content(string uid, string headline, DataSource dataSource, ContentType contentType, MediaType mediaType, License license, User owner) : this(uid, headline, dataSource.Code, contentType, mediaType, license, owner)
    {
        this.DataSourceId = dataSource?.Id ?? throw new ArgumentNullException(nameof(dataSource));
        this.DataSource = dataSource;
    }

    public Content(string uid, string headline, DataSource dataSource, int contentTypeId, int mediaTypeId, int licenseId, int ownerId) : this(uid, headline, dataSource.Code, contentTypeId, mediaTypeId, licenseId, ownerId)
    {
        this.DataSourceId = dataSource?.Id ?? throw new ArgumentNullException(nameof(dataSource));
        this.DataSource = dataSource;
    }

    public Content(string uid, string headline, string source, int? dataSourceId, int contentTypeId, int mediaTypeId, int licenseId, int ownerId) : this(uid, headline, source, contentTypeId, mediaTypeId, licenseId, ownerId)
    {
        this.DataSourceId = dataSourceId;
    }
    #endregion
}
