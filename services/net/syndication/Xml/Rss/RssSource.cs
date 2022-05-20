using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.Syndication.Xml;

/// <summary>
/// RssSource class, provides an object to read or write RSS source element.
/// </summary>
[XmlRoot("source")]
public class RssSource
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [XmlIgnore]
    public string Value { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlAttribute("url")]
    public Uri Url { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssSource object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssSource(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            this.Value = element.Value ?? throw new XmlException("Source element value is required.");

            var link = element.Element("link")?.Value ?? throw new XmlException("Source attribute 'url' is required.");
            if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out Uri? uri)) this.Url = uri;
            else throw new XmlException("Source attribute 'url' is not a valid URI");
        }
        else
        {
            this.Value = element.Value ?? "";
            var link = element.Element("link")?.Value ?? "";
            if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out Uri? uri)) this.Url = uri;
            else this.Url = new Uri("/");
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the source element from the specified 'item'.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssSource? Load(XElement item, bool enforceSpec = true)
    {
        var source = item.Element("source");
        return source != null ? new RssSource(source, enforceSpec) : null;
    }

    /// <summary>
    /// Casts RssSource to SyndicationFeed.
    /// </summary>
    /// <param name="source"></param>
    public static implicit operator SyndicationFeed?(RssSource? source)
    {
        if (source == null) return null;
        return new SyndicationFeed(source.Value, null, source.Url);
    }
    #endregion
}
