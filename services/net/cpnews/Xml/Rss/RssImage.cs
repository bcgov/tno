using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.CPNews.Xml;

/// <summary>
/// RssImage class, provides an object to read or write a RSS image.
/// </summary>
[XmlRoot("image")]
public class RssImage
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("url")]
    public Uri Url { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("title")]
    public string Title { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("link")]
    public Uri Link { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("width")]
    public int Width { get; set; } = 88;

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("height")]
    public int Height { get; set; } = 31;

    /// <summary>
    /// get/set -
    /// </summary>
    [XmlElement("description")]
    public string? Description { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RssImage object, initializes with specified parameter.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="enforceSpec"></param>
    /// <exception cref="XmlException"></exception>
    public RssImage(XElement element, bool enforceSpec = true)
    {
        if (Int32.TryParse(element.Element("width")?.Value, out int width)) this.Width = width;
        if (Int32.TryParse(element.Element("height")?.Value, out int height)) this.Height = height;

        if (enforceSpec)
        {
            var url = element.Element("url")?.Value ?? throw new XmlException("Image element 'url' is required.");
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri? imageUrl)) this.Url = imageUrl;
            else throw new XmlException("Image element 'url' is not a valid URI");

            this.Title = element.Element("title")?.Value ?? throw new XmlException("Image element 'title' required.");

            var link = element.Element("link")?.Value ?? throw new XmlException("Image element 'link' is required.");
            if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out Uri? linkUrl)) this.Link = linkUrl;
            else throw new XmlException("Image element 'link' is not a valid URI");

            if (width > 144) throw new XmlException("Image element width maximum is 144");
            if (height > 144) throw new XmlException("Image element height maximum is 400");
        }
        else
        {
            var url = element.Element("url")?.Value ?? "";
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri? imageUrl)) this.Url = imageUrl;
            else this.Url = new Uri("/");

            this.Title = element.Element("title")?.Value ?? "";

            var link = element.Element("link")?.Value ?? "";
            if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out Uri? linkUrl)) this.Link = linkUrl;
            else this.Link = new Uri("/");
        }

        this.Description = element.Element("description")?.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extracts the image element from the specified 'channelOrItem'.
    /// </summary>
    /// <param name="channelOrItem"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    public static RssImage? Load(XElement channelOrItem, bool enforceSpec = true)
    {
        var image = channelOrItem.Element("image");
        return image != null ? new RssImage(image, enforceSpec) : null;
    }

    /// <summary>
    /// Casts RssImage to Uri.
    /// </summary>
    /// <param name="image"></param>
    public static implicit operator Uri?(RssImage? image)
    {
        return image?.Url;
    }

    /// <summary>
    /// Casts RssImage to SyndicationLink.
    /// </summary>
    /// <param name="image"></param>
    public static implicit operator SyndicationLink(RssImage image)
    {
        return new SyndicationLink(image.Url)
        {
            Title = image.Title,
        };
    }
    #endregion
}
