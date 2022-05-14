namespace TNO.Models.Kafka;

public abstract class ContentBase
{
    #region Properties
    public string Source { get; set; } = "";
    public SourceMediaType MediaType { get; set; }
    public string Uid { get; set; } = "";
    public string Link { get; set; } = "";
    public string Language { get; set; } = "";
    public string Copyright { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string StreamUrl { get; set; } = "";
    public DateTime? PublishedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public IEnumerable<Author> Authors { get; set; } = Array.Empty<Author>();
    public IEnumerable<Tag> Tags { get; set; } = Array.Empty<Tag>();
    #endregion

    #region Constructors
    public ContentBase() { }

    public ContentBase(SourceMediaType mediaType, string source, string uid, string title, string summary)
    {
        this.MediaType = mediaType;
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        this.Title = title ?? throw new ArgumentNullException(nameof(title));
        this.Summary = summary ?? throw new ArgumentNullException(nameof(summary));
    }

    public ContentBase(SourceMediaType mediaType, string source, string uid, string title, string summary, DateTime publishedOn) : this(mediaType, source, uid, title, summary)
    {
        this.PublishedOn = publishedOn;
    }
    #endregion
}
