using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using AwesomeAssertions;
using TNO.Services.Syndication.Xml;

namespace TNO.Test.Services.Syndication;

public class AtomFeedTest
{
    private const string Feed = """
        <feed xmlns="http://www.w3.org/2005/Atom">
            <id>feed-1</id>
            <title>News</title>
            <entry>
                <id>story-1</id>
                <title>Story</title>
                <content type="html"><![CDATA[ <pre>  first
          second  </pre> ]]></content>
            </entry>
        </feed>
        """;

    [Theory]
    [InlineData("document")]
    [InlineData("xmlDocument")]
    [InlineData("reader")]
    public void Load_PreservesEntriesAndContent(string source)
    {
        var document = XDocument.Parse(Feed, LoadOptions.PreserveWhitespace);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(Feed);
        using var reader = XmlReader.Create(new StringReader(Feed));

        SyndicationFeed feed = source switch
        {
            "document" => AtomFeed.Load(document),
            "xmlDocument" => AtomFeed.Load(xmlDocument),
            _ => AtomFeed.Load(reader),
        };

        feed.Id.Should().Be("feed-1");
        feed.Title.Text.Should().Be("News");
        var item = feed.Items.Should().ContainSingle().Subject;
        item.Id.Should().Be("story-1");
        item.Title.Text.Should().Be("Story");
        ((TextSyndicationContent)item.Content).Text.Should().Be(
            document.Descendants(XName.Get("content", "http://www.w3.org/2005/Atom")).Single().Value);
    }

    [Theory]
    [InlineData("<atom />")]
    [InlineData("<feed />")]
    [InlineData("<feed xmlns='urn:other' />")]
    [InlineData("<html />")]
    [InlineData("<rss version='2.0' />")]
    public void Load_RejectsNonAtomDocuments(string xml)
    {
        var document = XDocument.Parse(xml);

        AtomFeed.IsAtomFeed(document).Should().BeFalse();
        var load = () => AtomFeed.Load(document);
        load.Should().Throw<XmlException>();
    }
}
