using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
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

public class SyndicationContentTypeTest
{
    [Theory]
    [InlineData(null, ContentType.Internet)]
    [InlineData("null", ContentType.Internet)]
    [InlineData("\"\"", ContentType.Internet)]
    [InlineData("\"PrintContent\"", ContentType.PrintContent)]
    [InlineData("\"Internet\"", ContentType.Internet)]
    [InlineData("\"AudioVideo\"", ContentType.AudioVideo)]
    [InlineData("\"Image\"", ContentType.Image)]
    public async Task ImportFeeds_AppliesOverrideToEveryFeedAndPreservesMediaTypes(string? overrideJson, ContentType expected)
    {
        var (action, manager, messages) = CreateImport(overrideJson);

        await action.PerformActionAsync<object>(manager);

        messages.Should().HaveCount(2);
        messages.Should().OnlyContain(content => content.ContentType == expected);
        messages.Select(content => content.Source).Should().Equal("AGASSIZ", "SECOND");
        messages.Select(content => content.MediaTypeId).Should().Equal(2, 9);
        manager.Ingest.IngestType!.ContentType.Should().Be(ContentType.Internet);
    }

    [Theory]
    [InlineData("\"UnknownType\"")]
    [InlineData("\"999\"")]
    public async Task ImportFeeds_InvalidOverrideDoesNotPublishContent(string overrideJson)
    {
        var (action, manager, messages) = CreateImport(overrideJson);

        var run = () => action.PerformActionAsync<object>(manager);

        await run.Should().ThrowAsync<InvalidOperationException>().WithMessage("*invalid content type override*");
        messages.Should().BeEmpty();
    }

    private static (SyndicationAction Action, IIngestActionManager Manager, List<SourceContent> Messages) CreateImport(string? overrideJson)
    {
        // Deserialize as the API does: configuration values arrive as JsonElements.
        var configuration = JsonSerializer.Deserialize<Dictionary<string, object>>("""
            {
                "post": true,
                "feeds": [
                    { "url": "https://example.test/first", "source": "AGASSIZ" },
                    { "url": "https://example.test/second", "source": "SECOND" }
                ]
            }
            """)!;
        if (overrideJson != null) configuration["contentType"] = JsonSerializer.Deserialize<JsonElement>(overrideJson);
        var ingest = new IngestModel
        {
            Name = "Blackpress",
            Topic = "BLACKPRESS",
            Source = new SourceModel { Code = "BLACKPRESS" },
            MediaTypeId = 9,
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
        api.Setup(a => a.GetResponseDataAsync<SourceModel[]>(sourceResponse))
            .ReturnsAsync([new SourceModel { Code = "AGASSIZ", MediaTypeId = 2 }]);
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
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <rss version="2.0"><channel><title>Weekly news</title>
                    <link>https://example.test/</link><description>News feed</description>
                    <item><title>Local news</title><guid>story-1</guid>
                    <description>Story text</description></item></channel></rss>
                    """),
            });
        var action = new SyndicationAction(http.Object, api.Object,
            Options.Create(new SyndicationOptions()), NullLogger<SyndicationAction>.Instance);
        return (action, manager.Object, messages);
    }
}
