using System.Globalization;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using TNO.Core.Extensions;

namespace TNO.Services.Syndication.Xml;

/// <summary>
/// RssItem class, provides an object to read or write RSS category element.
/// </summary>
[XmlRoot("item")]
public class RssItem
{
    // this namespace is for iPolitics syndication content format
    private static XNamespace contentXNamespace = "http://purl.org/rss/1.0/modules/content/";
    private static XNamespace dcXNamespace = "http://purl.org/dc/elements/1.1/";
    #region Properties
    /// <summary>
    /// get/set - The title of the item.
    /// </summary>
    [XmlElement("title")]
    public string? Title { get; set; }

    /// <summary>
    /// get/set - The URL of the item.
    /// </summary>
    [XmlElement("link")]
    public Uri? Link { get; set; }

    /// <summary>
    /// get/set - The item synopsis.
    /// </summary>
    [XmlElement("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// get/set - The item content.
    /// </summary>
    [XmlElement("content")]
    public string? Content { get; set; }

    /// <summary>
    /// get/set - Email address of the author of the item. More.
    /// </summary>
    [XmlElement("author")]
    public string? Author { get; set; }

    /// <summary>
    /// get/set - Includes the item in one or more categories. More.
    /// </summary>
    [XmlElement("category")]
    public ICollection<RssCategory> Categories { get; set; } = new List<RssCategory>();

    /// <summary>
    /// get/set - URL of a page for comments relating to the item. More.
    /// </summary>
    [XmlElement("comments")]
    public Uri? Comments { get; set; }

    /// <summary>
    /// get/set - Describes a media object that is attached to the item. More.
    /// </summary>
    [XmlElement("enclosure")]
    public RssEnclosure? Enclosure { get; set; }

    /// <summary>
    /// get/set - A string that uniquely identifies the item. More.
    /// </summary>
    [XmlElement("guid")]
    public RssGuid? Guid { get; set; }

    /// <summary>
    /// get/set - Indicates when the item was published. More.
    /// </summary>
    [XmlElement("pubDate")]
    public DateTimeOffset? PublishedOn { get; set; }

    /// <summary>
    /// get/set - The RSS channel that the item came from. More.
    /// </summary>
    [XmlElement("source")]
    public RssSource? Source { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssItem object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssItem(XElement element, bool enforceSpec = true)
    {
        this.Title = element.Element("title")?.Value;
        this.Description = element.Element("description")?.Value;
        
        var contentList = element.Descendants(contentXNamespace + "encoded");
        if (contentList != null && contentList.Any())
        {
            this.Content = contentList.FirstOrDefault()?.ToString();
        }

        if (enforceSpec)
        {
            var link = element.Element("link")?.Value ?? throw new XmlException("Channel element 'link' is required.");
            if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out Uri? uri)) this.Link = uri;
            else throw new XmlException("Channel element 'link' is not a valid URI");

            if (String.IsNullOrWhiteSpace(this.Title) && String.IsNullOrWhiteSpace(this.Description)) throw new XmlException("Item element must have 'title' or 'description' element.");
        }
        else
        {
            var link = element.Element("link")?.Value ?? "";
            if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out Uri? uri)) this.Link = uri;
            else this.Link = new Uri("/");
        }

        this.Author = element.Element("author")?.Value;
        if (this.Author == null || string.IsNullOrEmpty(this.Author))
        {
            var creator = element.Descendants(dcXNamespace + "creator");
            if (creator != null && creator.Any())
            {
                this.Author = creator.FirstOrDefault()?.Value.ToString();
            }
        }

        var comments = element.Element("comments")?.Value;
        if (Uri.TryCreate(comments, UriKind.RelativeOrAbsolute, out Uri? commentUri)) this.Comments = commentUri;

        this.Guid = RssGuid.Load(element, enforceSpec);
        this.Enclosure = RssEnclosure.Load(element, enforceSpec);
        this.Categories = RssCategory.LoadAll(element, enforceSpec);
        this.Source = RssSource.Load(element, enforceSpec);

        if (element.Element("pubDate")?.Value.TryParseDateTimeExact(CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime pubDate, RssFeed.PUB_DATE_FORMATS) == true)
            this.PublishedOn = pubDate;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the item elements from the specified 'channel'.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static ICollection<RssItem> LoadAll(XElement channel, bool enforceSpec = true)
    {
        var items = channel.Elements("item");
        return items?.Select(c => new RssItem(c, enforceSpec)).ToList() ?? new List<RssItem>();
    }

    /// <summary>
    /// Extracts the item from the specified 'element'.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssItem Load(XElement element, bool enforceSpec = true)
    {
        return new RssItem(element, enforceSpec);
    }

    /// <summary>
    /// Casts RssItem to SyndicationItem.
    /// </summary>
    /// <param name="item"></param>
    public static implicit operator SyndicationItem(RssItem item)
    {
        var result = new SyndicationItem
        {
            Id = item.Guid,
            Title = new TextSyndicationContent(item.Title, TextSyndicationContentKind.Html),
            Summary = new TextSyndicationContent(item.Description, TextSyndicationContentKind.Html),
            Content = new TextSyndicationContent(item.Content, TextSyndicationContentKind.Html),
            Copyright = null,
            SourceFeed = item.Source,
        };
        item.Categories.ForEach(c => result.Categories.Add(c));
        if (!String.IsNullOrWhiteSpace(item.Author)) {
            if (item.Author.Contains("@"))  // if the author contains "@" assume it is email
            {
                result.Authors.Add(new SyndicationPerson(item.Author));
            }
            else  // otherwise, add author string as name
            {
                result.Authors.Add(new SyndicationPerson(null, item.Author, null));
            }
        }
        if (item.PublishedOn != null) result.PublishDate = item.PublishedOn.Value;
        result.Links.Add(new SyndicationLink(item.Link, "alternate", null, null, 0));
        if (item.Enclosure != null) result.Links.Add(item.Enclosure);
        if (item.Comments != null) result.Links.Add(new SyndicationLink(item.Comments, "comments", null, null, 0));

        return result;
    }
    #endregion
}
