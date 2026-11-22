using System.Net;
using System.Net.Http.Headers;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Areas.Kafka.Models;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Http;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.Syndication;
using TNO.Services.Syndication.Config;

namespace TNO.Test.Services.Syndication;

public class SyndicationUpdatedDateTest
{
    [Fact]
    public void NormalizeUpdatedDates_ConvertsRfc822ValueToAtomDate()
    {
        var document = XDocument.Parse("""
            <rss xmlns:atom="http://www.w3.org/2005/Atom"><channel><item>
            <pubDate>Sat, 21 Nov 2026 15:00:00 GMT</pubDate>
            <atom:updated>Mon, 09 Nov 2026 21:29:00 GMT</atom:updated>
            </item></channel></rss>
            """);

        SyndicationAction.NormalizeUpdatedDates(document);

        var updated = document.Descendants(XName.Get("updated", "http://www.w3.org/2005/Atom")).Single();
        updated.Value.Should().Be("2026-11-09T21:29:00Z");
    }

    [Fact]
    public void NormalizeUpdatedDates_RemovesUnparseableValue()
    {
        var document = XDocument.Parse("""
            <rss xmlns:atom="http://www.w3.org/2005/Atom"><channel><item>
            <atom:updated>yesterday afternoon</atom:updated>
            </item></channel></rss>
            """);

        SyndicationAction.NormalizeUpdatedDates(document);

        document.Descendants(XName.Get("updated", "http://www.w3.org/2005/Atom"))
            .Should().BeEmpty();
    }

    [Fact]
    public void NormalizeUpdatedDates_PreservesValidValue()
    {
        var data = """
            <rss xmlns:atom="http://www.w3.org/2005/Atom"><channel><item>
            <atom:updated>2026-11-09T21:29:00-08:00</atom:updated>
            </item></channel></rss>
            """;

        var document = XDocument.Parse(data, LoadOptions.PreserveWhitespace);
        SyndicationAction.NormalizeUpdatedDates(document);

        document.ToString(SaveOptions.DisableFormatting).Should().Be(data);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ImportFeeds_InvalidAtomUpdatedDate_PublishesItem(bool customFeed)
    {
        var configuration = JsonSerializer.Deserialize<Dictionary<string, object>>($$"""
            {
                "post": true,
                "url": "https://www.princegeorgecitizen.com/rss",
                "customFeed": {{customFeed.ToString().ToLowerInvariant()}}
            }
            """)!;
        var ingest = new IngestModel
        {
            Name = "Prince George Citizen",
            Topic = "PGC",
            Source = new SourceModel { Code = "PGC" },
            IngestType = new IngestTypeModel { Name = "Syndication", ContentType = ContentType.Internet },
            Configuration = configuration,
            RetryLimit = 1,
        };
        var manager = new Mock<IIngestActionManager>();
        manager.SetupGet(m => m.Ingest).Returns(ingest);
        manager.Setup(m => m.UpdateIngestStateFailedAttemptsAsync(It.IsAny<int>(), It.IsAny<DateTime?>())).ReturnsAsync(ingest);

        var api = new Mock<IApiService>();
        var sourceResponse = new HttpResponseMessage(HttpStatusCode.OK);
        api.Setup(a => a.GetSourcesResponseAsync()).ReturnsAsync(sourceResponse);
        api.Setup(a => a.GetResponseDataAsync<SourceModel[]>(sourceResponse)).ReturnsAsync([]);
        api.Setup(a => a.AddContentReferenceAsync(It.IsAny<ContentReferenceModel>()))
            .ReturnsAsync((ContentReferenceModel reference) => reference);
        api.Setup(a => a.UpdateContentReferenceAsync(It.IsAny<ContentReferenceModel>()))
            .ReturnsAsync((ContentReferenceModel reference) => reference);
        var messages = new List<SourceContent>();
        api.Setup(a => a.SendMessageAsync(ingest.Topic, It.IsAny<SourceContent>()))
            .Callback<string, SourceContent>((_, content) => messages.Add(content))
            .ReturnsAsync(new DeliveryResultModel<SourceContent>());

        var http = new Mock<IHttpRequestClient>();
        http.Setup(h => h.SendAsync(It.IsAny<Uri>(), HttpMethod.Get, It.IsAny<HttpRequestHeaders>(), null, null))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <?xml version="1.0" encoding="utf-8"?>
                    <rss xmlns:atom="http://www.w3.org/2005/Atom" version="2.0"><channel>
                    <title>Prince George Citizen</title><link>https://www.princegeorgecitizen.com</link><description>News</description>
                    <item><title>Local news</title><link>https://www.princegeorgecitizen.com/news/1</link>
                    <guid>story-1</guid><description>Story text</description>
                    <pubDate>Sat, 21 Nov 2026 15:00:00 GMT</pubDate>
                    <atom:updated>Mon, 09 Nov 2026 21:29:00 GMT</atom:updated>
                    </item></channel></rss>
                    """),
            });
        var action = new SyndicationAction(http.Object, api.Object,
            Options.Create(new SyndicationOptions()), NullLogger<SyndicationAction>.Instance);

        await action.PerformActionAsync<object>(manager.Object, cancellationToken: TestContext.Current.CancellationToken);

        var message = messages.Should().ContainSingle().Subject;
        message.Title.Should().Be("Local news");
        message.PublishedOn.Should().Be(new DateTime(2026, 11, 21, 15, 0, 0, DateTimeKind.Utc));
        message.UpdatedOn.Should().Be(new DateTime(2026, 11, 9, 21, 29, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void StandardReader_WithoutNormalization_ThrowsForRfc822UpdatedDate()
    {
        using var reader = System.Xml.XmlReader.Create(new StringReader("""
            <rss xmlns:atom="http://www.w3.org/2005/Atom" version="2.0"><channel>
            <title>Prince George Citizen</title><link>https://www.princegeorgecitizen.com</link>
            <item><title>Local news</title><link>https://www.princegeorgecitizen.com/news/1</link>
            <pubDate>Sat, 21 Nov 2026 15:00:00 GMT</pubDate>
            <atom:updated>Mon, 09 Nov 2026 21:29:00 GMT</atom:updated>
            </item></channel></rss>
            """));
        var item = SyndicationFeed.Load(reader).Items.Single();

        item.Invoking(i => _ = i.LastUpdatedTime).Should().Throw<XmlException>();
    }
}
