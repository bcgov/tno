using System.Reflection;
using System.Text;
using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TNO.API.Helpers;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Ches.Models;
using TNO.Core.Http;
using TNO.DAL;
using TNO.Elastic;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services;
using TNO.Services.Indexing;
using TNO.Services.Indexing.Config;
using Xunit;

namespace TNO.Test.API;

public class IndexFailureEmailTest
{
    [Fact]
    public async Task ApiEmailIncludesAffectedIdsAndUsesConfiguredSupport()
    {
        var ches = new Mock<IChesService>();
        ches.Setup(c => c.SendEmailAsync(It.IsAny<IEmail>())).ReturnsAsync(new EmailResponseModel());
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Service:EmailTo"] = "support@example.test, operations@example.test" }).Build();
        var notifier = new IndexFailureNotifier(ches.Object, Options.Create(new ChesOptions { From = "mmi@example.test" }), configuration, NullLogger<IndexFailureNotifier>.Instance);
        await notifier.NotifyAsync([new SavedIndexRequest(9265437, IndexRequestAction.Delete, 3, "lifecycle", null)], new Exception("Kafka <unavailable>"));
        ches.Verify(c => c.SendEmailAsync(It.Is<IEmail>(e => e.To.Contains("support@example.test") && e.To.Contains("operations@example.test") && e.Body.Contains("9265437") && e.Body.Contains("Delete") && e.Body.Contains("&lt;unavailable&gt;"))), Times.Once);
    }

    [Fact]
    public async Task ApiEmailFailureDoesNotReplaceOriginalError()
    {
        var ches = new Mock<IChesService>();
        ches.Setup(c => c.SendEmailAsync(It.IsAny<IEmail>())).ThrowsAsync(new Exception("CHES unavailable"));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Service:EmailTo"] = "support@example.test" }).Build();
        var notifier = new IndexFailureNotifier(ches.Object, Options.Create(new ChesOptions()), configuration, NullLogger<IndexFailureNotifier>.Instance);
        await notifier.NotifyAsync([], new Exception("Kafka unavailable"));
    }

    [Theory]
    [InlineData(503, 1)]
    [InlineData(404, 0)]
    [InlineData(409, 0)]
    public async Task ExhaustedDeleteFailureEmailsSupportButMissingAndVersionConflictsDoNot(int status, int expectedEmails)
    {
        var api = new Mock<IApiService>();
        var openClient = new Mock<IOpenIdConnectRequestClient>();
        using var http = new HttpClient();
        openClient.SetupGet(c => c.Client).Returns(http);
        api.SetupGet(c => c.OpenClient).Returns(openClient.Object);
        var ches = new Mock<IChesService>();
        ches.Setup(c => c.SendEmailAsync(It.IsAny<IEmail>())).ReturnsAsync(new EmailResponseModel());
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var manager = new IndexingManager(Mock.Of<IKafkaAdmin>(), Mock.Of<IKafkaListener<string, IndexRequestModel>>(), api.Object, ches.Object,
            Options.Create(new ElasticOptions { Url = new Uri("http://localhost:9200") }), Options.Create(new ChesOptions { From = "mmi@example.test" }),
            Options.Create(new IndexingOptions { SendEmailOnFailure = true, EmailTo = "support@example.test", IndexRetryLimit = 2, IndexRetryDelayMs = 0 }), NullLogger<IndexingManager>.Instance, cache);
        var connection = new global::Elastic.Transport.InMemoryConnection(Encoding.UTF8.GetBytes("{\"error\":{\"type\":\"test_error\",\"reason\":\"test failure\"},\"status\":" + status + "}"), status);
        var client = new ElasticsearchClient(new ElasticsearchClientSettings(new SingleNodePool(new Uri("http://localhost:9200")), connection).DisablePing().MaximumRetries(0));
        typeof(IndexingManager).GetProperty("Client", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(manager, client);
        var method = typeof(IndexingManager).GetMethod("DeleteContentAsync", BindingFlags.NonPublic | BindingFlags.Instance, [typeof(long), typeof(string), typeof(long?)])!;
        await (Task)method.Invoke(manager, new object?[] { 9265437L, "published_content", 3L })!;
        ches.Verify(c => c.SendEmailAsync(It.Is<IEmail>(e => e.To.Contains("support@example.test") && e.Body.Contains("9265437") && e.Body.Contains("published_content"))), Times.Exactly(expectedEmails));
    }
}
