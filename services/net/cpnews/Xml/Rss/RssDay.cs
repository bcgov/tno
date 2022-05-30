using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.CPNews.Xml;

/// <summary>
/// RssDay class, provides an object to read and write RSS day element.
/// </summary>
[XmlRoot("day")]
public class RssDay
{
    #region Values
    /// <summary>
    /// The days of the week.
    /// </summary>
    public readonly static string[] Days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    #endregion

    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [XmlIgnore]
    public string Value { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssDay object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssDay(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            this.Value = element?.Value ?? throw new XmlException("Day element value required.");

            if (!Days.Contains(this.Value)) throw new XmlException("Day element must be a valid day of the week");
        }
        else
        {
            this.Value = element?.Value ?? "";
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the day element from the specified 'channel'.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static ICollection<RssDay> LoadAll(XElement channel, bool enforceSpec = true)
    {
        var day = channel.Element("skipDays")?.Elements("day");
        return day?.Select(c => new RssDay(c, enforceSpec)).ToList() ?? new List<RssDay>();
    }

    /// <summary>
    /// Extracts the day element from the specified 'day'.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public static RssDay Load(XElement day)
    {
        return new RssDay(day);
    }
    #endregion
}
