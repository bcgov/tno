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

public class SyndicationXmlDeclarationTest
{
    private const string DebugCommentFeed = """
        <!-- THEME_DEBUG -->
        <!-- THEME_HOOK: 'page__rss' -->
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0"><channel><title>Ha-Shilth-Sa</title>
        <link>https://hashilthsa.com/</link><description>News</description>
        <item><title>Local news</title><link>https://hashilthsa.com/news/1</link>
        <guid>story-1</guid><description>Story text</description></item></channel></rss>
        """;

    [Fact]
    public void NormalizeXmlDeclaration_PreservesDeclarationAndLeadingComments()
    {
        var normalized = SyndicationAction.NormalizeXmlDeclaration(" \r\n<!-- THEME_DEBUG -->\n<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<rss />");

        normalized.Should().Be("<?xml version=\"1.0\" encoding=\"utf-8\"?> \r\n<!-- THEME_DEBUG -->\n\r\n<rss />");
        var document = System.Xml.Linq.XDocument.Parse(normalized);
        document.Declaration!.Encoding.Should().Be("utf-8");
        document.Root!.Name.LocalName.Should().Be("rss");
    }

    [Theory]
    [InlineData("<?xml version=\"1.0\"?><rss />")]
    [InlineData("<rss />")]
    [InlineData("<html><?xml version=\"1.0\"?></html>")]
    [InlineData("<!-- unterminated\n<?xml version=\"1.0\"?><rss />")]
    [InlineData("<!-- comment -->\n<?xml version=\"1.0\"<rss />")]
    [InlineData("<?xml-stylesheet type=\"text/xsl\"?><rss />")]
    public void NormalizeXmlDeclaration_LeavesOtherDocumentsUnchanged(string data)
    {
        SyndicationAction.NormalizeXmlDeclaration(data).Should().BeSameAs(data);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ImportFeeds_CommentsBeforeXmlDeclaration_PublishesItem(bool customFeed)
    {
        var configuration = JsonSerializer.Deserialize<Dictionary<string, object>>($$"""
            {
                "post": true,
                "url": "https://hashilthsa.com/feed",
                "customFeed": {{customFeed.ToString().ToLowerInvariant()}}
            }
            """)!;
        var ingest = new IngestModel
        {
            Name = "Ha-Shilth-Sa",
            Topic = "HASHILTHSA",
            Source = new SourceModel { Code = "HS" },
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
                Content = new StringContent(DebugCommentFeed),
            });
        var action = new SyndicationAction(http.Object, api.Object,
            Options.Create(new SyndicationOptions()), NullLogger<SyndicationAction>.Instance);

        await action.PerformActionAsync<object>(manager.Object, cancellationToken: TestContext.Current.CancellationToken);

        var message = messages.Should().ContainSingle().Subject;
        message.Title.Should().Be("Local news");
        message.Link.Should().Be("https://hashilthsa.com/news/1");
        http.Verify(h => h.SendAsync(It.IsAny<Uri>(), HttpMethod.Get, It.IsAny<HttpRequestHeaders>(), null, null), Times.Once);
    }
}
