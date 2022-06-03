using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using TNO.Core.Extensions;

namespace TNO.Services.Models.Xml;

/// <summary>
/// RssFeed class, provides an object to read and write RSS syndication feed.
/// </summary>
/// <link>https://validator.w3.org/feed/docs/rss2.html#ltenclosuregtSubelementOfLtitemgt</link>
[XmlRoot("rss")]
public class RssFeed
{
    #region Variables
    #endregion

    #region Properties
    /// <summary>
    /// get - The XML document.
    /// </summary>
    [XmlIgnore]
    protected XDocument Document { get; private set; }

    /// <summary>
    /// get - The RSS version.
    /// </summary>
    [XmlAttribute("version")]
    public string Version { get { return this.Document.Root?.Attribute("version")?.Value ?? throw new XmlException("Attribute 'version' is required."); } }

    /// <summary>
    /// get - The channel element.
    /// </summary>
    [XmlElement("channel")]
    public RssChannel Channel { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssFeed object, initializes with specified parameter.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="enforceSpec"></param>
    /// <exception type="XmlException">Not a valid RSS feed</exception>
    public RssFeed(XmlReader reader, bool enforceSpec = true) : this(XDocument.Load(reader), enforceSpec) { }

    /// <summary>
    /// Creates a new instance of a RssFeed object, initializes with specified parameter.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <exception type="XmlException">Not a valid RSS feed</exception>
    public RssFeed(XmlDocument document, bool enforceSpec = true)
    {
        using var reader = new XmlNodeReader(document);
        reader.MoveToContent();
        this.Document = XDocument.Load(reader);
        this.Channel = RssChannel.Load(this.Document, enforceSpec);
    }

    /// <summary>
    /// Creates a new instance of a RssFeed object, initializes with specified parameter.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <exception type="XmlException">Not a valid RSS feed</exception>
    public RssFeed(XDocument document, bool enforceSpec = true)
    {
        this.Document = document;
        this.Channel = RssChannel.Load(document, enforceSpec);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Check if the specified document is an RSS feed.
    /// This doesn't validate the whole document.
    /// The purpose is to determine which syndication type the document is (RSS, Atom).
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public static bool IsRssFeed(XmlDocument document)
    {
        using var reader = new XmlNodeReader(document);
        reader.MoveToContent();
        return IsRssFeed(XDocument.Load(reader));
    }

    /// <summary>
    /// Check if the specified document is an RSS feed.
    /// This doesn't validate the whole document.
    /// The purpose is to determine which syndication type the document is (RSS, Atom).
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public static bool IsRssFeed(XDocument document)
    {
        return document.Root?.Name == "rss";
    }

    /// <summary>
    /// Load the RSS feed from the XDocument.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    /// <exception type="XmlException">Not a valid RSS feed</exception>
    public static RssFeed Load(XDocument document, bool enforceSpec = true)
    {
        if (!IsRssFeed(document)) throw new XmlException("Not a valid RSS feed.");
        return new RssFeed(document, enforceSpec);
    }

    /// <summary>
    /// Load the RSS feed from the XmlDocument.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    /// <exception type="XmlException">Not a valid RSS feed</exception>
    public static RssFeed Load(XmlDocument document, bool enforceSpec = true)
    {
        using var reader = new XmlNodeReader(document);
        reader.MoveToContent();
        var xml = XDocument.Load(reader);
        if (!IsRssFeed(xml)) throw new XmlException("Not a valid RSS feed.");
        return new RssFeed(xml, enforceSpec);
    }

    /// <summary>
    /// Load the RSS feed from the XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    /// <exception type="XmlException">Not a valid RSS feed</exception>
    public static RssFeed Load(XmlReader reader, bool enforceSpec = true)
    {
        var document = XDocument.Load(reader);
        if (!IsRssFeed(document)) throw new XmlException("Not a valid RSS feed.");
        return new RssFeed(reader, enforceSpec);
    }

    /// <summary>
    /// Casts RssFeed to SyndicationFeed.
    /// </summary>
    /// <param name="feed"></param>
    public static implicit operator SyndicationFeed(RssFeed feed)
    {
        var channel = feed.Channel;
        var result = new SyndicationFeed()
        {
            Id = "",
            BaseUri = channel.Link,
            Title = new TextSyndicationContent(channel.Title, TextSyndicationContentKind.Html),
            Description = new TextSyndicationContent(channel.Description, TextSyndicationContentKind.Html),
            Copyright = new TextSyndicationContent(channel.Copyright, TextSyndicationContentKind.Html),
            Documentation = channel.Documentation != null ? new SyndicationLink(channel.Documentation) : null,
            Generator = channel.Generator,
            ImageUrl = channel.Image,
            Language = channel.Language,
            TextInput = channel.TextInput
        };
        channel.Categories.ForEach(c => result.Categories.Add(c));
        channel.SkipHours.ForEach(sh => result.SkipHours.Add(sh.Value));
        channel.SkipDays.ForEach(sd => result.SkipDays.Add(sd.Value));
        if (channel.TimeToLive != null) result.TimeToLive = new TimeSpan(0, channel.TimeToLive.Value, 0);
        if (!String.IsNullOrWhiteSpace(channel.ManagingEditor)) result.Contributors.Add(new SyndicationPerson(channel.ManagingEditor));
        if (!String.IsNullOrWhiteSpace(channel.WebMaster)) result.Contributors.Add(new SyndicationPerson(channel.WebMaster));
        if (channel.LastBuildDate != null) result.LastUpdatedTime = channel.LastBuildDate.Value;
        result.Links.Add(new SyndicationLink(channel.Link));
        result.Items = channel.Items.Select(i =>
        {
            var item = (SyndicationItem)i;
            if (item.Links.Any(l => !l.Uri.IsAbsoluteUri))
                item.BaseUri = result.BaseUri;
            return item;
        });

        return result;
    }
    #endregion
}
