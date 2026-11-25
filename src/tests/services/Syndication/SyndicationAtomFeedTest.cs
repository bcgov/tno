using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
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

public class SyndicationAtomFeedTest
{
    // Reduced Vista Radio response: indentation and CDATA must survive the XML document round trip.
    private const string AtomFeed = """
        <?xml version="1.0" encoding="UTF-8" ?>
        <feed xmlns="http://www.w3.org/2005/Atom">
            <title>Vista Radio</title>
            <id>https://bc.news.vistaradio.ca/</id>
            <updated>2026-11-23T12:14:48Z</updated>
            <entry>
                <id>story-1</id>
                <title type="html">Local news</title>
                <link href="https://example.com/news/1" rel="alternate" />
                <published>2026-11-23T02:41:59-05:00</published>
                <updated>2026-11-23T03:41:59-05:00</updated>
                <summary>Story summary</summary>
                <content type="html"><![CDATA[<p>Story &amp; details</p>]]></content>
                <author><name>Reporter</name></author>
            </entry>
        </feed>
        """;

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task ImportFeeds_IndentedAtom_PublishesItem(bool customFeed, bool withDtd)
    {
        var configuration = JsonSerializer.Deserialize<Dictionary<string, object>>($$"""
            {
                "post": true,
                "url": "https://bc.news.vistaradio.ca/wprss",
                "customFeed": {{customFeed.ToString().ToLowerInvariant()}}
            }
            """)!;
        var ingest = new IngestModel
        {
            Name = "Vista Radio",
            Topic = "VISTA",
            Source = new SourceModel { Code = "VISTA" },
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
                Content = new StringContent(withDtd
                    ? AtomFeed.Replace("<feed xmlns=", "<!DOCTYPE feed [<!ENTITY story 'Local news'>]><feed xmlns=")
                        .Replace(">Local news</title>", ">&story;</title>")
                    : AtomFeed),
            });
        var action = new SyndicationAction(http.Object, api.Object,
            Options.Create(new SyndicationOptions()), NullLogger<SyndicationAction>.Instance);

        await action.PerformActionAsync<object>(manager.Object, cancellationToken: TestContext.Current.CancellationToken);

        var message = messages.Should().ContainSingle().Subject;
        message.Title.Should().Be("Local news");
        message.Link.Should().Be("https://example.com/news/1");
        message.PublishedOn.Should().Be(new DateTime(2026, 11, 23, 7, 41, 59, DateTimeKind.Utc));
        message.UpdatedOn.Should().Be(new DateTime(2026, 11, 23, 8, 41, 59, DateTimeKind.Utc));
        http.Verify(h => h.SendAsync(It.IsAny<Uri>(), HttpMethod.Get, It.IsAny<HttpRequestHeaders>(), null, null), Times.Once);
    }
}
