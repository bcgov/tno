using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.CPNews.Xml;

/// <summary>
/// RssEnclosure class, provides an object to read or write RSS enclosure element.
/// </summary>
[XmlRoot("enclosure")]
public class RssEnclosure
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [XmlAttribute("url")]
    public Uri Url { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlAttribute("length")]
    public long Length { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlAttribute("type")]
    public string Type { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlIgnore]
    public string? Value { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssEnclosure object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssEnclosure(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            var url = element.Attribute("url")?.Value ?? throw new XmlException("Enclosure attribute 'url' is required.");
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri? uri)) this.Url = uri;
            else throw new XmlException("Enclosure attribute 'url' is not a valid URI");

            var length = element.Attribute("length")?.Value ?? throw new XmlException("Enclosure attribute 'length' is required.");
            if (long.TryParse(length, out long value)) this.Length = value;

            this.Type = element.Attribute("type")?.Value ?? throw new XmlException("Enclosure attribute 'type' is required.");
        }
        else
        {
            var url = element.Attribute("url")?.Value ?? "";
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri? uri)) this.Url = uri;
            else this.Url = new Uri("/");

            var length = element.Attribute("length")?.Value ?? "";
            if (long.TryParse(length, out long value)) this.Length = value;

            this.Type = element.Attribute("type")?.Value ?? "";
        }

        this.Value = element.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the enclosure element from the specified 'item'.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssEnclosure? Load(XElement item, bool enforceSpec = true)
    {
        var enclosure = item.Element("enclosure");
        return enclosure != null ? new RssEnclosure(enclosure, enforceSpec) : null;
    }

    /// <summary>
    /// Casts RssEnclosure to SyndicationLink.
    /// </summary>
    /// <param name="enclosure"></param>
    public static implicit operator SyndicationLink?(RssEnclosure? enclosure)
    {
        if (enclosure == null) return null;
        return new SyndicationLink(enclosure.Url, "enclosure", enclosure.Value, enclosure.Type, enclosure.Length);
    }
    #endregion
}
