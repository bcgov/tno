using System.ServiceModel.Syndication;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.Models.Xml;

/// <summary>
/// RssTextInput class, provides an object to read and write a RSS textInput.
/// </summary>
[XmlRoot("textInput")]
public class RssTextInput
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("title")]
    public string Title { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("description")]
    public string Description { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("name")]
    public string Name { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("link")]
    public Uri Link { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssTextInput object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssTextInput(XElement element, bool enforceSpec = true)
    {
        if (enforceSpec)
        {
            this.Title = element.Element("title")?.Value ?? throw new XmlException("TextInput element 'title' required.");
            this.Description = element.Element("description")?.Value ?? throw new XmlException("TextInput element 'description' required.");
            this.Name = element.Element("name")?.Value ?? throw new XmlException("TextInput element 'name' required.");

            var link = element.Element("link")?.Value ?? throw new XmlException("TextInput element 'link' is required.");
            if (Uri.TryCreate(link, UriKind.Absolute, out Uri? linkUri)) this.Link = linkUri;
            else throw new XmlException("TextInput element 'link' is not a valid URI");
        }
        else
        {
            this.Title = element.Element("title")?.Value ?? "";
            this.Description = element.Element("description")?.Value ?? "";
            this.Name = element.Element("name")?.Value ?? "";

            var link = element.Element("link")?.Value ?? "";
            if (Uri.TryCreate(link, UriKind.Absolute, out Uri? linkUri)) this.Link = linkUri;
            else this.Link = new Uri("/");
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
    public static RssTextInput? Load(XElement channel, bool enforceSpec = true)
    {
        var textInput = channel.Element("textInput");
        return textInput != null ? new RssTextInput(textInput, enforceSpec) : null;
    }

    /// <summary>
    /// Casts RssTextInput to SyndicationTextInput.
    /// </summary>
    /// <param name="textInput"></param>
    public static implicit operator SyndicationTextInput?(RssTextInput? textInput)
    {
        if (textInput == null) return null;

        var result = new SyndicationTextInput()
        {
            Description = textInput.Description,
            Title = textInput.Title,
            Name = textInput.Name,
            Link = new SyndicationLink(textInput.Link)
        };

        return result;
    }
    #endregion
}
