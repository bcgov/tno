using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TNO.DAL.Services;

namespace TNO.Test.DAL.Elastic;

/// <summary>
/// ReportEvidenceService: only a missing evidence index falls back to no evidence; every other failure stops the report.
/// </summary>
public class ReportEvidenceServiceTest
{
    private static ReportEvidenceService Service(FakeElasticServer server)
        => new(FakeElasticServer.Client(server.Url), Options.Create(new TNO.Elastic.ElasticOptions { Url = server.Url, EvidenceIndex = "content_evidence" }),
            Options.Create(new JsonSerializerOptions(JsonSerializerDefaults.Web)), NullLogger<ReportEvidenceService>.Instance);

    [Fact]
    public async Task FindAsync_MissingIndexReturnsNoEvidence()
    {
        using var server = new FakeElasticServer(_ => new(404, "{\"error\":{\"type\":\"index_not_found_exception\"},\"status\":404}"));

        var result = await Service(server).FindAsync([1, 2], TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    public async Task FindAsync_OtherFailuresThrow(int status)
    {
        using var server = new FakeElasticServer(_ => new(status, "{\"error\":\"failed\"}"));

        var act = () => Service(server).FindAsync([1], TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage($"*HTTP {status}*");
    }

    [Fact]
    public async Task FindAsync_UnreachableThrows()
    {
        var url = new Uri($"http://127.0.0.1:{FakeElasticServer.FreePort()}/");
        var service = new ReportEvidenceService(FakeElasticServer.Client(url), Options.Create(new TNO.Elastic.ElasticOptions { Url = url }),
            Options.Create(new JsonSerializerOptions(JsonSerializerDefaults.Web)), NullLogger<ReportEvidenceService>.Instance);

        var act = () => service.FindAsync([1], TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*no response*");
    }

    [Fact]
    public async Task FindAsync_CancelledThrows()
    {
        using var server = new FakeElasticServer(_ => new(200, "{\"id\":\"pit\"}", TimeSpan.FromSeconds(30)));
        using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        var act = () => Service(server).FindAsync([1], cancel.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task FindAsync_ReadsApprovedEvidenceAndClosesThePointInTime()
    {
        using var server = new FakeElasticServer(request => request switch
        {
            { Method: "POST", PathAndQuery: "/content_evidence/_pit?keep_alive=1m" } => new(200, "{\"id\":\"pit-1\"}"),
            { Method: "POST", PathAndQuery: "/_search" } => new(200,
                "{\"pit_id\":\"pit-1\",\"hits\":{\"hits\":[{\"_source\":{\"contentId\":7,\"isApproved\":true},\"sort\":[7]}]}}"),
            // The point in time may already have expired; that is not an error.
            { Method: "DELETE", PathAndQuery: "/_pit" } => new(404, "{\"succeeded\":false,\"num_freed\":0}"),
            _ => new(400, "{\"error\":\"unexpected\"}"),
        });

        var result = await Service(server).FindAsync([7], TestContext.Current.CancellationToken);

        result.Keys.Should().Equal(7L);
        var search = server.Requests.Single(r => r.PathAndQuery == "/_search");
        search.Body.Should().Contain("\"isApproved\":true").And.Contain("\"pit\"");
        server.Requests.Should().Contain(r => r.Method == "DELETE" && r.PathAndQuery == "/_pit" && r.Body.Contains("pit-1"));
    }
}
