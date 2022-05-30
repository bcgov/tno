using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.CPNews.Xml;

/// <summary>
/// RssHour class, provides an object to read and write RSS hours element.
/// </summary>
[XmlRoot("hours")]
public class RssHour
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [XmlIgnore]
    public int Value { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssHour object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssHour(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            if (Int32.TryParse(element?.Value, out int value)) this.Value = value;
            else throw new XmlException("Hour element value required.");

            if (value < 0 || value > 23) throw new XmlException("Hour element must be value between 0 and 23");
        }
        else
        {
            if (Int32.TryParse(element?.Value, out int value)) this.Value = value;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the hours element from the specified 'channel'.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static ICollection<RssHour> LoadAll(XElement channel, bool enforceSpec = true)
    {
        var hours = channel.Element("skipHours")?.Elements("hours");
        return hours?.Select(c => new RssHour(c, enforceSpec)).ToList() ?? new List<RssHour>();
    }

    /// <summary>
    /// Extracts the hours element from the specified 'hours'.
    /// </summary>
    /// <param name="hours"></param>
    /// <returns></returns>
    public static RssHour Load(XElement hours)
    {
        return new RssHour(hours);
    }
    #endregion
}
