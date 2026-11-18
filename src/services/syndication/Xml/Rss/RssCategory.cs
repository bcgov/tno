using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.Syndication.Xml;

/// <summary>
/// RssCategory class, provides an object to read or write RSS category element.
/// </summary>
[XmlRoot("category")]
public class RssCategory
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
    [XmlAttribute("domain")]
    public string? Domain { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssCategory object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssCategory(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            this.Value = element.Value ?? throw new XmlException("Category element value required.");
        }
        else
        {
            this.Value = element.Value ?? "";
        }

        this.Domain = element.Attribute("domain")?.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the category elements from the specified 'channelOrItem'.
    /// </summary>
    /// <param name="channelOrItem"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static ICollection<RssCategory> LoadAll(XElement channelOrItem, bool enforceSpec = true)
    {
        var categories = channelOrItem.Elements("category");
        return categories?.Select(c => new RssCategory(c, enforceSpec)).ToList() ?? new List<RssCategory>();
    }

    /// <summary>
    /// Extracts the category element from the specified 'element'.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssCategory Load(XElement element, bool enforceSpec = true)
    {
        return new RssCategory(element, enforceSpec);
    }

    /// <summary>
    /// Casts RssCategory to SyndicationCategory.
    /// </summary>
    /// <param name="category"></param>
    public static implicit operator SyndicationCategory(RssCategory category)
    {
        var result = new SyndicationCategory
        {
            Name = category.Value,
            Scheme = category.Domain
        };

        return result;
    }
    #endregion
}
