using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.Syndication.Xml;

/// <summary>
/// RssCloud class, provides an object to read and write RSS cloud element.
/// </summary>
[XmlRoot("cloud")]
public class RssCloud
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [XmlAttribute("domain")]
    public string Domain { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlAttribute("port")]
    public int Port { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlAttribute("path")]
    public string Path { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlAttribute("registerProcedure")]
    public string RegisterProcedure { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlAttribute("protocol")]
    public string Protocol { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssCloud object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssCloud(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            this.Domain = element.Attribute("domain")?.Value ?? throw new XmlException("Cloud attribute 'domain' required.");
            if (Int32.TryParse(element.Attribute("port")?.Value, out int port)) this.Port = port;
            else throw new XmlException("Cloud attribute 'port' required.");
            this.Path = element.Attribute("path")?.Value ?? throw new XmlException("Cloud attribute 'path' required.");
            this.RegisterProcedure = element.Attribute("registerProcedure")?.Value ?? throw new XmlException("Cloud attribute 'registerProcedure' required.");
            this.Protocol = element.Attribute("protocol")?.Value ?? throw new XmlException("Cloud attribute 'protocol' required.");
        }
        else
        {
            this.Domain = element.Attribute("domain")?.Value ?? "";
            if (Int32.TryParse(element.Attribute("port")?.Value, out int port)) this.Port = port;
            this.Path = element.Attribute("path")?.Value ?? "";
            this.RegisterProcedure = element.Attribute("registerProcedure")?.Value ?? "";
            this.Protocol = element.Attribute("protocol")?.Value ?? "";
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the cloud element from the specified 'channel'.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssCloud? Load(XElement channel, bool enforceSpec = true)
    {
        var category = channel.Element("cloud");
        return category != null ? new RssCloud(category, enforceSpec) : null;
    }
    #endregion
}
