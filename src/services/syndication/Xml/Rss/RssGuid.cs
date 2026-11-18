using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.Syndication.Xml;

/// <summary>
/// RssGuid class, provides an object to read or write RSS guid element.
/// </summary>
[XmlRoot("guid")]
public class RssGuid
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
    [XmlAttribute("isPermaLink")]
    public bool IsPermaLink { get; set; } = true;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssGuid object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssGuid(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            this.Value = element.Value ?? throw new XmlException("Guid element value is required.");
        }
        else
        {
            this.Value = element.Value ?? "";
        }
        if (Boolean.TryParse(element.Attribute("isPermaLink")?.Value, out bool isPermaLink)) this.IsPermaLink = isPermaLink;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the guid element from the specified 'item'.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssGuid? Load(XElement item, bool enforceSpec = true)
    {
        var guid = item.Element("guid");
        return guid != null ? new RssGuid(guid, enforceSpec) : null;
    }

    /// <summary>
    /// Casts RssGuid to String.
    /// </summary>
    /// <param name="guid"></param>
    public static implicit operator string?(RssGuid? guid)
    {
        return guid?.Value;
    }
    #endregion
}
