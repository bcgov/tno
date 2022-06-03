using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.Models.Xml;

/// <summary>
/// RssChannel class, provides an object to read or write RSS channel elements.
/// </summary>
[XmlRoot("channel")]
public class RssChannel
{
    #region Properties
    /// <summary>
    /// get/set - The name of the channel. It's how people refer to your service. If you have an HTML website that contains the same information as your RSS file, the title of your channel should be the same as the title of your website.
    /// </summary>
    [XmlElement("title")]
    public string Title { get; set; }

    /// <summary>
    /// get/set - The URL to the HTML website corresponding to the channel.
    /// </summary>
    [XmlElement("link")]
    public Uri Link { get; set; }

    /// <summary>
    /// get/set - Phrase or sentence describing the channel.
    /// </summary>
    [XmlElement("description")]
    public string Description { get; set; }

    /// <summary>
    /// get/set - The language the channel is written in. This allows aggregators to group all Italian language sites, for example, on a single page. A list of allowable values for this element, as provided by Netscape, is here. You may also use values defined by the W3C.
    /// </summary>
    [XmlElement("language")]
    public string? Language { get; set; }

    /// <summary>
    /// get/set - Copyright notice for content in the channel.
    /// </summary>
    [XmlElement("copyright")]
    public string? Copyright { get; set; }

    /// <summary>
    /// get/set - Email address for person responsible for editorial content.
    /// </summary>
    [XmlElement("managingEditor")]
    public string? ManagingEditor { get; set; }

    /// <summary>
    /// get/set - Email address for person responsible for technical issues relating to channel.
    /// </summary>
    [XmlElement("webMaster")]
    public string? WebMaster { get; set; }

    /// <summary>
    /// get/set - The publication date for the content in the channel. For example, the New York Times publishes on a daily basis, the publication date flips once every 24 hours. That's when the pubDate of the channel changes. All date-times in RSS conform to the Date and Time Specification of RFC 822, with the exception that the year may be expressed with two characters or four characters (four preferred).
    /// </summary>
    [XmlElement("pubDate")]
    public DateTimeOffset? PublishedOn { get; set; }

    /// <summary>
    /// get/set - The last time the content of the channel changed.
    /// </summary>
    [XmlElement("lastBuildDate")]
    public DateTimeOffset? LastBuildDate { get; set; }

    /// <summary>
    /// get/set - Specify one or more categories that the channel belongs to. Follows the same rules as the item-level category element. More info.
    /// </summary>
    [XmlElement("category")]
    public ICollection<RssCategory> Categories { get; set; } = new List<RssCategory>();

    /// <summary>
    /// get/set - A string indicating the program used to generate the channel.
    /// </summary>
    [XmlElement("generator")]
    public string? Generator { get; set; }

    /// <summary>
    /// get/set - A URL that points to the documentation for the format used in the RSS file. It's probably a pointer to this page. It's for people who might stumble across an RSS file on a Web server 25 years from now and wonder what it is.
    /// </summary>
    [XmlElement("docs")]
    public Uri? Documentation { get; set; }

    /// <summary>
    /// get/set - Allows processes to register with a cloud to be notified of updates to the channel, implementing a lightweight publish-subscribe protocol for RSS feeds. More info here.
    /// </summary>
    [XmlElement("cloud")]
    public RssCloud? Cloud { get; set; }

    /// <summary>
    /// get/set - ttl stands for time to live. It's a number of minutes that indicates how long a channel can be cached before refreshing from the source. More info here.
    /// </summary>
    [XmlElement("ttl")]
    public int? TimeToLive { get; set; }

    /// <summary>
    /// get/set - Specifies a GIF, JPEG or PNG image that can be displayed with the channel. More info here.
    /// </summary>
    [XmlElement("image")]
    public RssImage? Image { get; set; }

    /// <summary>
    /// get/set - The PICS rating for the channel.
    /// </summary>
    [XmlElement("rating")]
    public RssRating? Rating { get; set; }

    /// <summary>
    /// get/set - Specifies a text input box that can be displayed with the channel. More info here.
    /// </summary>
    [XmlElement("textInput")]
    public RssTextInput? TextInput { get; set; }

    /// <summary>
    /// get/set - A hint for aggregators telling them which hours they can skip. This element contains up to 24 hour sub-elements whose value is a number between 0 and 23, representing a time in GMT, when aggregators, if they support the feature, may not read the channel on hours listed in the skipHours element. The hour beginning at midnight is hour zero.
    /// </summary>
    [XmlArray("skipHours")]
    public ICollection<RssHour> SkipHours { get; set; } = new List<RssHour>();

    /// <summary>
    /// get/set - A hint for aggregators telling them which days they can skip. This element contains up to seven day sub-elements whose value is Monday, Tuesday, Wednesday, Thursday, Friday, Saturday or Sunday. Aggregators may not read the channel during days listed in the skipDays element.
    /// </summary>
    [XmlArray("skipDays")]
    public ICollection<RssDay> SkipDays { get; set; } = new List<RssDay>();

    /// <summary>
    /// get/set - A channel may contain any number of items. An item may represent a "story" -- much like a story in a newspaper or magazine; if so its description is a synopsis of the story, and the link points to the full story. An item may also be complete in itself, if so, the description contains the text (entity-encoded HTML is allowed; see examples), and the link and title may be omitted. All elements of an item are optional, however at least one of title or description must be present.
    /// </summary>
    [XmlElement("item")]
    public ICollection<RssItem> Items { get; set; } = new List<RssItem>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssChannel object, initializes with specified parameter.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssChannel(XDocument document, bool enforceSpec = true) : this(document.Element("rss")?.Element("channel") ?? throw new XmlException("Channel element is required"), enforceSpec) { }

    /// <summary>
    /// Creates a new instance of a RssChannel object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssChannel(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            this.Title = element.Element("title")?.Value ?? throw new XmlException("Channel element 'title' is required.");

            var link = element.Element("link")?.Value ?? throw new XmlException("Channel element 'link' is required.");
            if (Uri.TryCreate(link, UriKind.Absolute, out Uri? linkUri)) this.Link = linkUri;
            else throw new XmlException("Channel element 'link' is not a valid URI");
            this.Description = element.Element("description")?.Value ?? throw new XmlException("Channel element 'description' is required.");
        }
        else
        {
            this.Title = element.Element("title")?.Value ?? "";

            var link = element.Element("link")?.Value ?? "";
            if (Uri.TryCreate(link, UriKind.Absolute, out Uri? linkUri)) this.Link = linkUri;
            else this.Link = new Uri("/");
            this.Description = element.Element("description")?.Value ?? "";
        }

        // Optional
        this.Language = element.Element("language")?.Value;
        this.Copyright = element.Element("copyright")?.Value;
        this.ManagingEditor = element.Element("managingEditor")?.Value;
        this.WebMaster = element.Element("webMaster")?.Value;
        this.Generator = element.Element("generator")?.Value;

        var docs = element.Element("docs")?.Value;
        if (Uri.TryCreate(docs, UriKind.Absolute, out Uri? docsUri)) this.Documentation = docsUri;

        if (DateTime.TryParse(element.Element("pubDate")?.Value, out DateTime pubDate)) this.PublishedOn = pubDate;
        if (DateTime.TryParse(element.Element("lastBuildDate")?.Value, out DateTime lastBuildDate)) this.LastBuildDate = lastBuildDate;
        if (Int32.TryParse(element.Element("ttl")?.Value, out int ttl)) this.TimeToLive = ttl;

        this.Categories = RssCategory.LoadAll(element, enforceSpec);
        this.Cloud = RssCloud.Load(element, enforceSpec);

        this.Image = RssImage.Load(element, enforceSpec);
        this.Rating = RssRating.Load(element, enforceSpec);
        this.TextInput = RssTextInput.Load(element, enforceSpec);
        this.SkipHours = RssHour.LoadAll(element, enforceSpec);
        this.SkipDays = RssDay.LoadAll(element, enforceSpec);

        this.Items = RssItem.LoadAll(element, enforceSpec);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the channel element from the specified 'element'.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssChannel Load(XElement element, bool enforceSpec = true)
    {
        var channel = element.Element("channel") ?? throw new XmlException("Channel element is required");
        return new RssChannel(channel, enforceSpec);
    }

    /// <summary>
    /// Extracts the channel element from the specified 'document'.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssChannel Load(XDocument document, bool enforceSpec = true)
    {
        var channel = document.Element("rss")?.Element("channel") ?? throw new XmlException("Channel element is required");
        return new RssChannel(channel, enforceSpec);
    }
    #endregion
}
