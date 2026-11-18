namespace TNO.Services.Dedup;

/// <summary>
/// DedupEntry class, the comparable fingerprint of one story.
/// </summary>
public class DedupEntry
{
    #region Properties
    /// <summary>
    /// get - The source code of the story.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// get - The unique identifier of the story within the source.
    /// </summary>
    public string Uid { get; }

    /// <summary>
    /// get - The normalized headline.
    /// </summary>
    public string Headline { get; }

    /// <summary>
    /// get - The significant words in the headline.
    /// </summary>
    public HashSet<string> HeadlineTokens { get; }

    /// <summary>
    /// get - The fingerprint of the story text, or null when the text is too short to fingerprint.
    /// </summary>
    public ulong? BodyHash { get; }

    /// <summary>
    /// get - When the story was published (UTC).
    /// </summary>
    public DateTime PublishedOn { get; }

    /// <summary>
    /// get - The priority of the feed the story came from, lower is more important.
    /// </summary>
    public int Priority { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DedupEntry object, initializes with already normalized values.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <param name="headline">A normalized headline.</param>
    /// <param name="bodyHash"></param>
    /// <param name="publishedOn"></param>
    /// <param name="priority"></param>
    public DedupEntry(string source, string uid, string headline, ulong? bodyHash, DateTime publishedOn, int priority)
    {
        this.Source = source;
        this.Uid = uid;
        this.Headline = headline;
        this.HeadlineTokens = TextNormalizer.HeadlineTokens(headline);
        this.BodyHash = bodyHash;
        this.PublishedOn = publishedOn.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(publishedOn, DateTimeKind.Utc) : publishedOn.ToUniversalTime();
        this.Priority = priority;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Create a DedupEntry for a story from its raw headline and text.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <param name="headline">The headline as published (may contain markup or entities).</param>
    /// <param name="body">The story text as published (may be HTML).</param>
    /// <param name="publishedOn"></param>
    /// <param name="priority"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static DedupEntry Create(string source, string uid, string? headline, string? body, DateTime publishedOn, int priority, DedupOptions options)
    {
        var tokens = TextNormalizer.TextTokens(body);
        ulong? bodyHash = tokens.Length >= options.MinBodyTokens ? SimHash.Compute(tokens) : null;
        return new DedupEntry(source, uid, TextNormalizer.NormalizeHeadline(headline), bodyHash, publishedOn, priority);
    }

    /// <summary>
    /// Whether this is the specified story.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool IsSame(string source, string uid)
    {
        return this.Source == source && this.Uid == uid;
    }
    #endregion
}
