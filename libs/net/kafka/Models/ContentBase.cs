using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// ContentBase abstract class, provides a model for content sent to Kafka.
/// TODO: Change name to ContentBaseModel for consistent naming convention
/// </summary>
public abstract class ContentBase
{
    #region Properties

    /// <summary>
    /// get/set - A unique key to identify this content.
    /// </summary>
    public string Uid { get; set; } = "";

    /// <summary>
    /// get/set - A unique code to identify the data source the content belong to.
    /// </summary>
    public string Source { get; set; } = "";

    /// <summary>
    /// get/set - The type of content and form to use.
    /// </summary>
    public ContentType ContentType { get; set; }

    /// <summary>
    /// get/set - Foreign key to the product the content will be assigned by default.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// get/set - A URL to the content.
    /// TODO: Change to a Uri type.
    /// </summary>
    public string Link { get; set; } = "";

    /// <summary>
    /// get/set - The language of the content.
    /// </summary>
    public string Language { get; set; } = "";

    /// <summary>
    /// get/set - The copyright information for the content.
    /// </summary>
    public string Copyright { get; set; } = "";

    /// <summary>
    /// get/set - The title or headline to identify the content subject.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// get/set - The summary or abstract of the content.
    /// </summary>
    public string Summary { get; set; } = "";

    /// <summary>
    /// get/set - A data location is where the source was ingested and the files may reside.
    /// </summary>
    public string DataLocation { get; set; } = "";

    /// <summary>
    /// get/set - The path to the file associated with this content.
    /// This path will be relative to a data location that is available to the consuming service.
    /// Essentially the path is meaningless without the additional context, but is critical to fetch the file.
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// get/set - The section containing the content.
    /// </summary>
    public string Section { get; set; } = "";

    /// <summary>
    /// get/set - The page number containing the content.
    /// </summary>
    public string Page { get; set; } = "";

    /// <summary>
    /// get/set - A URL to a stream for this content.
    /// TODO: Change to a Uri type.
    /// </summary>
    public string StreamUrl { get; set; } = "";

    /// <summary>
    /// get/set - The date this content was published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - The date when this content was updated.
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// get/set - An array of author information.
    /// </summary>
    public IEnumerable<Author> Authors { get; set; } = Array.Empty<Author>();

    /// <summary>
    /// get/set - An array of tags to identify this content.
    /// </summary>
    public IEnumerable<Tag> Tags { get; set; } = Array.Empty<Tag>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentBase object.
    /// </summary>
    public ContentBase() { }

    /// <summary>
    /// Creates a new instance of a ContentBase object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataLocation"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="productId"></param>
    /// <param name="uid"></param>
    /// <param name="title"></param>
    /// <param name="summary"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ContentBase(string dataLocation, string source, ContentType contentType, int productId, string uid, string title, string summary)
    {
        this.DataLocation = dataLocation;
        this.ContentType = contentType;
        this.ProductId = productId;
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        this.Title = title ?? throw new ArgumentNullException(nameof(title));
        this.Summary = summary ?? throw new ArgumentNullException(nameof(summary));
    }

    /// <summary>
    /// Creates a new instance of a ContentBase object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataLocation"></param>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="productId"></param>
    /// <param name="uid"></param>
    /// <param name="title"></param>
    /// <param name="summary"></param>
    /// <param name="publishedOn"></param>
    public ContentBase(string dataLocation, string source, ContentType contentType, int productId, string uid, string title, string summary, DateTime publishedOn)
        : this(dataLocation, source, contentType, productId, uid, title, summary)
    {
        this.PublishedOn = publishedOn;
    }
    #endregion
}
