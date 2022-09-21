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
    /// get/set - Identifies the media type of this content.
    /// </summary>
    public SourceMediaType MediaType { get; set; }

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
    /// <param name="mediaType"></param>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <param name="title"></param>
    /// <param name="summary"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ContentBase(SourceMediaType mediaType, string source, string uid, string title, string summary)
    {
        this.MediaType = mediaType;
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        this.Title = title ?? throw new ArgumentNullException(nameof(title));
        this.Summary = summary ?? throw new ArgumentNullException(nameof(summary));
    }

    /// <summary>
    /// Creates a new instance of a ContentBase object, initializes with specified parameters.
    /// </summary>
    /// <param name="mediaType"></param>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <param name="title"></param>
    /// <param name="summary"></param>
    /// <param name="publishedOn"></param>
    public ContentBase(SourceMediaType mediaType, string source, string uid, string title, string summary, DateTime publishedOn) : this(mediaType, source, uid, title, summary)
    {
        this.PublishedOn = publishedOn;
    }
    #endregion
}
