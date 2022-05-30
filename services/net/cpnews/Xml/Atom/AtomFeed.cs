using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TNO.Services.CPNews.Xml;

/// <summary>
/// AtomFeed class, provides an object to read and write Atom syndication feed.
/// </summary>
[XmlRoot("atom")]
public class AtomFeed
{
    #region Variables
    #endregion

    #region Properties
    /// <summary>
    /// get - The XML document.
    /// </summary>
    [XmlIgnore]
    protected XDocument Document { get; private set; }

    /// <summary>
    /// get - The Atom version.
    /// </summary>
    [XmlAttribute("version")]
    public string Version { get { return this.Document.Root?.Attribute("version")?.Value ?? throw new XmlException("Attribute 'version' is required."); } }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AtomFeed object, initializes with specified parameter.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="enforceSpec"></param>
    /// <exception type="XmlException">Not a valid Atom feed</exception>
    public AtomFeed(XmlReader reader, bool enforceSpec = true) : this(XDocument.Load(reader), enforceSpec) { }

    /// <summary>
    /// Creates a new instance of a AtomFeed object, initializes with specified parameter.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <exception type="XmlException">Not a valid Atom feed</exception>
    public AtomFeed(XmlDocument document, bool enforceSpec = true)
    {
        using var reader = new XmlNodeReader(document);
        reader.MoveToContent();
        this.Document = XDocument.Load(reader);
    }

    /// <summary>
    /// Creates a new instance of a AtomFeed object, initializes with specified parameter.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <exception type="XmlException">Not a valid Atom feed</exception>
    public AtomFeed(XDocument document, bool enforceSpec = true)
    {
        this.Document = document;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Check if the specified document is an Atom feed.
    /// This doesn't validate the whole document.
    /// The purpose is to determine which syndication type the document is (Atom, Atom).
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public static bool IsAtomFeed(XmlDocument document)
    {
        using var reader = new XmlNodeReader(document);
        reader.MoveToContent();
        return IsAtomFeed(XDocument.Load(reader));
    }

    /// <summary>
    /// Check if the specified document is an Atom feed.
    /// This doesn't validate the whole document.
    /// The purpose is to determine which syndication type the document is (Atom, Atom).
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public static bool IsAtomFeed(XDocument document)
    {
        return document.Root?.Name == "atom";
    }

    /// <summary>
    /// Load the Atom feed from the XDocument.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    /// <exception type="XmlException">Not a valid Atom feed</exception>
    public static AtomFeed Load(XDocument document, bool enforceSpec = true)
    {
        if (!IsAtomFeed(document)) throw new XmlException("Not a valid Atom feed.");
        return new AtomFeed(document, enforceSpec);
    }

    /// <summary>
    /// Load the Atom feed from the XmlDocument.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    /// <exception type="XmlException">Not a valid Atom feed</exception>
    public static AtomFeed Load(XmlDocument document, bool enforceSpec = true)
    {
        using var reader = new XmlNodeReader(document);
        reader.MoveToContent();
        var xml = XDocument.Load(reader);
        if (!IsAtomFeed(xml)) throw new XmlException("Not a valid Atom feed.");
        return new AtomFeed(xml, enforceSpec);
    }

    /// <summary>
    /// Load the Atom feed from the XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="enforceSpec"></param>
    /// <returns></returns>
    /// <exception type="XmlException">Not a valid Atom feed</exception>
    public static AtomFeed Load(XmlReader reader, bool enforceSpec = true)
    {
        var document = XDocument.Load(reader);
        if (!IsAtomFeed(document)) throw new XmlException("Not a valid Atom feed.");
        return new AtomFeed(reader, enforceSpec);
    }

    /// <summary>
    /// Casts AtomFeed to SyndicationFeed.
    /// </summary>
    /// <param name="feed"></param>
    public static implicit operator SyndicationFeed(AtomFeed feed)
    {
        return new SyndicationFeed();
    }
    #endregion
}
