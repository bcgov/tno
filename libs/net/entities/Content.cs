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
    /// get/set - The status of the content.
    /// </summary>
    [Column("status")]
    public ContentStatus Status { get; set; }

    /// <summary>
    /// get/set - Identifies the type of content and the form to use.
    /// </summary>
    [Column("content_type")]
    public ContentType ContentType { get; set; }

    /// <summary>
    /// get/set - Foreign key to the source.
    /// </summary>
    [Column("source_id")]
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The source of the story.
    /// </summary>
    public virtual Source? Source { get; set; }

    /// <summary>
    /// get/set - The source code to identify the publisher.
    /// </summary>
    [Column("source")]
    public string OtherSource { get; set; } = "";

    /// <summary>
    /// get/set - The foreign key to the license.
    /// </summary>
    [Column("license_id")]
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set - The license to apply to this story.
    /// </summary>
    public virtual License? License { get; set; }

    /// <summary>
    /// get/set - Foreign key to the media type.
    /// </summary>
    [Column("media_type_id")]
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - The media type this content will be placed in.
    /// </summary>
    public virtual MediaType? MediaType { get; set; }

    /// <summary>
    /// get/set - Foreign key to the series.
    /// </summary>
    [Column("series_id")]
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set - The series this story belongs to.
    /// </summary>
    public virtual Series? Series { get; set; }

    /// <summary>
    /// get/set - Foreign key to the contributor.
    /// </summary>
    [Column("contributor_id")]
    public int? ContributorId { get; set; }

    /// <summary>
    /// get/set - The contributor this story belongs to.
    /// </summary>
    public virtual Contributor? Contributor { get; set; }

    /// <summary>
    /// get/set - Foreign key to the owner.
    /// </summary>
    [Column("owner_id")]
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The owner of the story.  Or the user responsible for it.
    /// </summary>
    public virtual User? Owner { get; set; }

    /// <summary>
    /// get/set - Story headline.
    /// </summary>
    [Column("headline")]
    public string Headline { get; set; } = "";

    /// <summary>
    /// get/set - The author or writer's name.
    /// </summary>
    [Column("byline")]
    public string Byline { get; set; } = "";

    /// <summary>
    /// get/set - Unique identifier within MMI based on a hash of values.
    /// </summary>
    [Column("uid")]
    public string Uid { get; set; } = "";

    /// <summary>
    /// get/set - Unique identifier from the external source if provided.
    /// </summary>
    [Column("external_uid")]
    public string ExternalUid { get; set; } = "";

    /// <summary>
    /// get/set - The print content edition.
    /// </summary>
    [Column("edition")]
    public string Edition { get; set; } = "";

    /// <summary>
    /// get/set - The section in the print content.
    /// </summary>
    [Column("section")]
    public string Section { get; set; } = "";

    /// <summary>
    /// get/set - The page this story was found one.
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
    [Column("posted_on")]
    public DateTime? PostedOn { get; set; }

    /// <summary>
    /// get/set - Story abstract or summary text.
    /// </summary>
    [Column("summary")]
    public string Summary { get; set; } = "";

    /// <summary>
    /// get/set - Story body text.
    /// </summary>
    [Column("body")]
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - The URL to the source story.
    /// </summary>
    [Column("source_url")]
    public string SourceUrl { get; set; } = "";

    /// <summary>
    /// get/set - Whether content is hidden from search results.
    /// </summary>
    [Column("is_hidden")]
    public bool IsHidden { get; set; }

    /// <summary>
    /// get/set - Whether the content has been approved for publishing.
    /// </summary>
    [Column("is_approved")]
    public bool IsApproved { get; set; }

    /// <summary>
    /// get/set - Private content is not searchable.
    /// </summary>
    [Column("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>
    /// get - Dictionary of versions associated with this content.
    /// This provides subscribers the ability to customize the content.
    /// The key is the user's ID.
    /// </summary>
    [Column("versions")]
    public Dictionary<int, Models.ContentVersion> Versions { get; set; } = new();

    /// <summary>
    /// get - Collection of logs associated with this content.
    /// </summary>
    public virtual List<ContentLog> Logs { get; } = new List<ContentLog>();

    /// <summary>
    /// get - Collection of topics associated with this content.
    /// </summary>
    public virtual List<Topic> Topics { get; } = new List<Topic>();

    /// <summary>
    /// get - Collection of topics, the many-to-many relationship.
    /// </summary>
    public virtual List<ContentTopic> TopicsManyToMany { get; } = new List<ContentTopic>();

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

    /// <summary>
    /// get - Collection of report instances associated with this content.
    /// </summary>
    public virtual List<ReportInstance> Reports { get; } = new List<ReportInstance>();

    /// <summary>
    /// get - Collection of report instances associated with this content (many-to-many).
    /// </summary>
    public virtual List<ReportInstanceContent> ReportsManyToMany { get; } = new List<ReportInstanceContent>();

    /// <summary>
    /// get - Collection of notification instances for this content (many-to-many).
    /// </summary>
    public virtual List<NotificationInstance> NotificationsManyToMany { get; } = new List<NotificationInstance>();

    /// <summary>
    /// get - Collection of users who want to be notified about this content.
    /// </summary>
    public virtual List<UserContentNotification> UserNotifications { get; } = new List<UserContentNotification>();

    /// <summary>
    /// get - Collection of folders that have this content.
    /// </summary>
    public virtual List<Folder> Folders { get; } = new List<Folder>();

    /// <summary>
    /// get - Collection of folders that have this content (many-to-many).
    /// </summary>
    public virtual List<FolderContent> FoldersManyToMany { get; } = new List<FolderContent>();

    /// <summary>
    /// get - Collection of work orders that are for this content.
    /// </summary>
    public virtual List<WorkOrder> WorkOrders { get; } = new List<WorkOrder>();

    /// <summary>
    /// get - Collection of Quotes associated with this content.
    /// </summary>
    public virtual List<Quote> Quotes { get; } = new List<Quote>();

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Content object.
    /// </summary>
    protected Content() { }

    /// <summary>
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="contentType"></param>
    /// <param name="license"></param>
    /// <param name="mediaType"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    private Content(string uid, string headline, ContentType contentType, License license, MediaType mediaType, User? owner = null)
    {
        if (String.IsNullOrWhiteSpace(headline)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(headline));

        this.Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        this.Headline = headline;
        this.ContentType = contentType;
        this.LicenseId = license?.Id ?? throw new ArgumentNullException(nameof(license));
        this.License = license;
        this.MediaTypeId = mediaType?.Id ?? throw new ArgumentNullException(nameof(mediaType));
        this.MediaType = mediaType;
        this.OwnerId = owner?.Id;
        this.Owner = owner;
        this.IsApproved = contentType != ContentType.AudioVideo;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="license"></param>
    /// <param name="mediaType"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string source, ContentType contentType, License license, MediaType mediaType, User? owner = null)
        : this(uid, headline, contentType, license, mediaType, owner)
    {
        if (String.IsNullOrWhiteSpace(source)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(source));

        this.OtherSource = source;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="contentType"></param>
    /// <param name="licenseId"></param>
    /// <param name="mediaTypeId"></param>
    /// <param name="ownerId"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    private Content(string uid, string headline, ContentType contentType, int licenseId, int mediaTypeId, int? ownerId = null)
    {
        this.Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        this.Headline = headline;
        this.ContentType = contentType;
        this.LicenseId = licenseId;
        this.MediaTypeId = mediaTypeId;
        this.OwnerId = ownerId;
        this.IsApproved = contentType != ContentType.AudioVideo;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="licenseId"></param>
    /// <param name="mediaTypeId"></param>
    /// <param name="ownerId"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string source, ContentType contentType, int licenseId, int mediaTypeId, int? ownerId = null)
        : this(uid, headline, contentType, licenseId, mediaTypeId, ownerId)
    {
        if (String.IsNullOrWhiteSpace(source)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(source));

        this.OtherSource = source;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="license"></param>
    /// <param name="mediaType"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, Source source, ContentType contentType, License license, MediaType mediaType, User? owner = null)
        : this(uid, headline, contentType, license, mediaType, owner)
    {
        this.SourceId = source?.Id ?? throw new ArgumentNullException(nameof(source));
        this.Source = source;
        this.OtherSource = source.Code;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="otherSource"></param>
    /// <param name="sourceId"></param>
    /// <param name="contentType"></param>
    /// <param name="licenseId"></param>
    /// <param name="mediaTypeId"></param>
    /// <param name="ownerId"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string otherSource, int? sourceId, ContentType contentType, int licenseId, int mediaTypeId, int? ownerId = null)
        : this(uid, headline, otherSource, contentType, licenseId, mediaTypeId, ownerId)
    {
        this.SourceId = sourceId;
    }

    // The constructor order here could make a difference for Elasticsearch!
    /// <summary>
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="otherSource"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="license"></param>
    /// <param name="mediaType"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string otherSource, Source source, ContentType contentType, License license, MediaType mediaType, User? owner = null)
        : this(uid, headline, otherSource, contentType, license, mediaType, owner)
    {
        this.SourceId = source?.Id ?? throw new ArgumentNullException(nameof(source));
        this.Source = source;
    }
    #endregion
}
