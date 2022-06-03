using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.Models.Xml;

/// <summary>
/// RssRating class, provides an object to read and write RSS rating element.
/// </summary>
[XmlRoot("rating")]
public class RssRating
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [XmlIgnore]
    public string Value { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssRating object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssRating(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            this.Value = element.Value ?? throw new XmlException("Rating element value required.");
        }
        else
        {
            this.Value = element.Value;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the image element from the specified 'channel'.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssRating? Load(XElement channel, bool enforceSpec = true)
    {
        var rating = channel.Element("rating");
        return rating != null ? new RssRating(rating, enforceSpec) : null;
    }
    #endregion
}
