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
    /// get/set - Foreign key to the product.
    /// </summary>
    [Column("product_id")]
    public int ProductId { get; set; }

    /// <summary>
    /// get/set - The product this content will be placed in.
    /// </summary>
    public virtual Product? Product { get; set; }

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
    /// get/set - Unique identifier from the source if possible.
    /// </summary>
    [Column("uid")]
    public string Uid { get; set; } = "";

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
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="contentType"></param>
    /// <param name="license"></param>
    /// <param name="product"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    private Content(string uid, string headline, ContentType contentType, License license, Product product, User? owner = null)
    {
        if (String.IsNullOrWhiteSpace(headline)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(headline));

        this.Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        this.Headline = headline;
        this.ContentType = contentType;
        this.LicenseId = license?.Id ?? throw new ArgumentNullException(nameof(license));
        this.License = license;
        this.ProductId = product?.Id ?? throw new ArgumentNullException(nameof(product));
        this.Product = product;
        this.OwnerId = owner?.Id;
        this.Owner = owner;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="license"></param>
    /// <param name="product"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string source, ContentType contentType, License license, Product product, User? owner = null)
        : this(uid, headline, contentType, license, product, owner)
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
    /// <param name="productId"></param>
    /// <param name="ownerId"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    private Content(string uid, string headline, ContentType contentType, int licenseId, int productId, int? ownerId = null)
    {
        if (String.IsNullOrWhiteSpace(headline)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(headline));

        this.Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        this.Headline = headline;
        this.ContentType = contentType;
        this.LicenseId = licenseId;
        this.ProductId = productId;
        this.OwnerId = ownerId;
    }

    /// <summary>
    /// Creates a new instance of a Content object, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="headline"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="licenseId"></param>
    /// <param name="productId"></param>
    /// <param name="ownerId"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string source, ContentType contentType, int licenseId, int productId, int? ownerId = null)
        : this(uid, headline, contentType, licenseId, productId, ownerId)
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
    /// <param name="product"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, Source source, ContentType contentType, License license, Product product, User? owner = null) : this(uid, headline, contentType, license, product, owner)
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
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="license"></param>
    /// <param name="product"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string otherSource, Source source, ContentType contentType, License license, Product product, User? owner = null) : this(uid, headline, otherSource, contentType, license, product, owner)
    {
        this.SourceId = source?.Id ?? throw new ArgumentNullException(nameof(source));
        this.Source = source;
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
    /// <param name="productId"></param>
    /// <param name="ownerId"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Content(string uid, string headline, string otherSource, int? sourceId, ContentType contentType, int licenseId, int productId, int? ownerId = null) : this(uid, headline, otherSource, contentType, licenseId, productId, ownerId)
    {
        this.SourceId = sourceId;
    }
    #endregion
}
