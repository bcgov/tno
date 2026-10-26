using System.Text;
using System.Text.Json;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Nest;
using TNO.API.Helpers;
using TNO.Core.Exceptions;
using TNO.DAL;
using TNO.Elastic;
using TNO.Entities;
using Xunit;

namespace TNO.Test.API;

public partial class OrphanedContentTest
{
    private sealed class ScanConnection(Func<RequestData, object> respond) : InMemoryConnection
    {
        public override Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Serialize(respond(requestData));
            return new InMemoryConnection(Encoding.UTF8.GetBytes(payload)).RequestAsync<TResponse>(requestData, cancellationToken);
        }
    }

    private static object Hits(params object[] hits) => new { _shards = new { failed = 0 }, timed_out = false, hits = new { hits } };
    private OrphanScanService Scans(TNOContext context, Func<RequestData, object>? respond = null, int grace = 0, int batch = 2)
    {
        var connection = new ScanConnection(respond ?? (_ => Hits()));
        var client = new ElasticClient(new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection).DisablePing());
        var elastic = new Mock<ITNOElasticClient>();
        elastic.SetupGet(x => x.LowLevel).Returns(client.LowLevel);
        var (repair, _) = Service(context, []);
        return new OrphanScanService(context, repair, elastic.Object, Options.Create(new OrphanScanOptions
        { BatchSize = batch, BatchDelayMilliseconds = 0, GracePeriodSeconds = grace }), NullLogger<OrphanScanService>.Instance);
    }

    private async Task AgeAsync(TNOContext context)
    {
        await context.Contents.ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedOn, DateTime.UtcNow.AddHours(-1)));
        context.ChangeTracker.Clear();
    }

    [DatabaseFact]
    public async Task BackgroundScanResumesAcrossScopesAndPagesConfirmedResults()
    {
        Guid id;
        await using (var context = Context())
        {
            for (var i = 0; i < 5; i++) Content(context);
            await AgeAsync(context);
            id = (await Scans(context).StartAsync(new("postgres"), default)).Id;
        }
        for (var i = 0; i < 6; i++)
        {
            await using var scope = Context();
            await Scans(scope).ProcessBatchAsync(default);
            // Bounded tracking per fresh worker scope, regardless of dataset size.
            Assert.InRange(scope.ChangeTracker.Entries().Count(), 0, 3);
        }
        await using var verify = Context();
        var service = Scans(verify);
        var status = await service.LatestAsync(default);
        Assert.Equal("Completed", status!.Status);
        Assert.Equal(5, status.Scanned);
        Assert.Equal(5, status.Found);
        Assert.Equal(0, status.Pending);
        var first = await service.ResultsAsync(id, 0, 2, default);
        var second = await service.ResultsAsync(id, first.NextAfter, 2, default);
        Assert.Equal(2, first.Items.Length);
        Assert.All(first.Items, x => Assert.Equal("Test", x.OtherSource));
        Assert.True(first.HasMore);
        Assert.All(second.Items, x => Assert.True(x.Id > first.NextAfter));
    }

    [DatabaseFact]
    public async Task ResultsLoadElasticsearchSourcesOnlyForTheDisplayedPage()
    {
        await using var context = Context();
        var job = new OrphanScan { Side = "elasticsearch", Index = "published", Status = "Completed", CompletedOn = DateTime.UtcNow };
        context.OrphanScans.Add(job);
        foreach (var id in new long[] { 9001, 9002, 9003 })
            context.OrphanScanResults.Add(new OrphanScanResult { ScanId = job.Id, ContentId = id, Confirmed = true, Headline = "Saved result" });
        await context.SaveChangesAsync();
        var requests = new List<JsonElement>();
        var service = Scans(context, request =>
        {
            using var stream = new MemoryStream();
            request.PostData!.Write(stream, request.ConnectionSettings);
            using var body = JsonDocument.Parse(stream.ToArray());
            requests.Add(body.RootElement.Clone());
            return Hits(new { _id = "9001", _source = new { otherSource = "Vancouver Sun" } }, new { _id = "9002", _source = new { } });
        });
        var results = await service.ResultsAsync(job.Id, 0, 2, default);
        var request = Assert.Single(requests);
        Assert.Equal(2, request.GetProperty("size").GetInt32());
        Assert.Contains("otherSource", request.GetProperty("_source").EnumerateArray().Select(x => x.GetString()));
        var ids = request.GetProperty("query").GetProperty("bool").GetProperty("filter")[0].GetProperty("ids").GetProperty("values");
        Assert.Equal(new[] { "9001", "9002" }, ids.EnumerateArray().Select(x => x.GetString()));
        Assert.Equal("Vancouver Sun", results.Items[0].OtherSource);
        Assert.Equal("", results.Items[1].OtherSource);
        Assert.True(results.HasMore);
        Assert.Equal(9002, results.NextAfter);

        var unavailable = Scans(context, _ => new { timed_out = true, _shards = new { failed = 0 }, hits = new { hits = Array.Empty<object>() } });
        var saved = await unavailable.ResultsAsync(job.Id, 0, 2, default);
        Assert.Equal(2, saved.Items.Length);
        Assert.All(saved.Items, x => Assert.Equal("Saved result", x.Headline));
        Assert.All(saved.Items, x => Assert.Equal("", x.OtherSource));
    }

    [DatabaseFact]
    public async Task CandidatesWaitForGraceAndDisappearIfIndexingCatchesUp()
    {
        long id;
        Guid scanId;
        await using (var context = Context())
        {
            id = Content(context).Id;
            await AgeAsync(context);
            var service = Scans(context, grace: 300);
            scanId = (await service.StartAsync(new("postgres"), default)).Id;
            await service.ProcessBatchAsync(default);
        }
        await using (var context = Context())
        {
            var service = Scans(context, grace: 300);
            await service.ProcessBatchAsync(default);
            Assert.Empty((await service.ResultsAsync(scanId, 0, 100, default)).Items);
            Assert.Equal(1, (await service.LatestAsync(default))!.Pending);
            await context.OrphanScanResults.ExecuteUpdateAsync(s => s.SetProperty(x => x.DetectedOn, DateTime.UtcNow.AddMinutes(-10)));
        }
        await using (var context = Context())
        {
            var service = Scans(context, _ => Hits(new { _id = id.ToString() }), grace: 300);
            await service.ProcessBatchAsync(default);
            var status = await service.LatestAsync(default);
            Assert.Equal("Completed", status!.Status);
            Assert.Equal(0, status.Found);
            Assert.Empty(await context.OrphanScanResults.ToArrayAsync());
        }
    }

    [DatabaseFact]
    public async Task ActiveJobAndAdvisoryLockPreventCompetingReplicas()
    {
        await using var first = Context();
        await Scans(first).StartAsync(new(), default);
        await using var second = Context();
        await Assert.ThrowsAsync<BadRequestException>(() => Scans(second).StartAsync(new(), default));
        await using var transaction = await first.Database.BeginTransactionAsync();
        await first.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(-73129481002)");
        await using var third = Context();
        await Scans(third, _ => throw new Exception("Must not reach Elasticsearch")).ProcessBatchAsync(default);
        Assert.Equal("Scanning", (await Scans(third).LatestAsync(default))!.Status);
    }

    [DatabaseFact]
    public async Task CancelPersistsAcrossScopesAndClosesSnapshot()
    {
        Guid id;
        await using (var context = Context())
        {
            id = (await Scans(context).StartAsync(new(), default)).Id;
            await context.OrphanScans.ExecuteUpdateAsync(s => s.SetProperty(x => x.PitId, "saved-snapshot"));
            await Scans(context).CancelAsync(id, default);
        }
        var closed = false;
        await using var next = Context();
        var service = Scans(next, r => { Assert.Contains("_pit", r.Uri.AbsolutePath); closed = true; return new { succeeded = true }; });
        await service.ProcessBatchAsync(default);
        Assert.True(closed);
        Assert.Equal("Cancelled", (await service.LatestAsync(default))!.Status);
        Assert.Null((await next.OrphanScans.SingleAsync()).PitId);
        Assert.NotEqual(id, (await service.StartAsync(new(), default)).Id);
    }

    [DatabaseFact]
    public async Task SnapshotUsesIdOnlySearchAfterAndRechecksLiveBeforeConfirmation()
    {
        var requests = new List<JsonElement>();
        var traversal = 0;
        object Respond(RequestData r)
        {
            if (r.Uri.AbsolutePath.EndsWith("/_pit") && r.Method == Elasticsearch.Net.HttpMethod.POST) return new { id = "snapshot-1" };
            if (r.Method == Elasticsearch.Net.HttpMethod.DELETE) return new { succeeded = true };
            using var stream = new MemoryStream();
            r.PostData!.Write(stream, r.ConnectionSettings);
            var body = JsonDocument.Parse(stream.ToArray()).RootElement.Clone();
            requests.Add(body);
            if (body.TryGetProperty("pit", out _))
            {
                traversal++;
                return new { pit_id = "snapshot-2", _shards = new { failed = 0 }, hits = new { hits = traversal == 1
                    ? new object[] { new { _id = "9001", sort = new[] { 41 } }, new { _id = "9002", sort = new[] { 42 } } }
                    : Array.Empty<object>() } };
            }
            // One candidate disappeared from ES before confirmation; only the other is orphaned.
            return Hits(new { _id = "9002", _source = new { id = 9002, headline = "Still orphaned", status = 3 } });
        }
        Guid id;
        await using (var context = Context()) id = (await Scans(context).StartAsync(new(), default)).Id;
        for (var i = 0; i < 3; i++)
        {
            await using var context = Context();
            await Scans(context, Respond).ProcessBatchAsync(default);
        }
        Assert.False(requests[0].GetProperty("_source").GetBoolean());
        Assert.Equal("_shard_doc", requests[0].GetProperty("sort")[0].GetString());
        Assert.Equal(42, requests[1].GetProperty("search_after")[0].GetInt32());
        Assert.Equal("snapshot-2", requests[1].GetProperty("pit").GetProperty("id").GetString());
        await using var verify = Context();
        var result = await Scans(verify).ResultsAsync(id, 0, 100, default);
        Assert.Equal(9002, Assert.Single(result.Items).Id);
        Assert.Equal("Completed", (await Scans(verify).LatestAsync(default))!.Status);
    }

    [DatabaseFact]
    public async Task PartialResultsFailWithoutSavingFalseOrphans()
    {
        Guid id;
        await using (var context = Context())
        {
            Content(context); await AgeAsync(context);
            id = (await Scans(context).StartAsync(new("postgres"), default)).Id;
        }
        await using var worker = Context();
        var service = Scans(worker, _ => new { timed_out = true, _shards = new { failed = 0 }, hits = new { hits = Array.Empty<object>() } });
        await service.ProcessBatchAsync(default);
        var job = await service.LatestAsync(default);
        Assert.Equal("Failed", job!.Status);
        Assert.Equal(0, job.Scanned);
        Assert.Empty((await service.ResultsAsync(id, 0, 100, default)).Items);
        Assert.NotNull(job.Error);
    }

    [DatabaseFact]
    public async Task PostedDateRangeIncludesWholePacificEndDayAndExcludesNullAndRecentContent()
    {
        await using var context = Context();
        // November 1 is a 25-hour Pacific day in 2026. IDs deliberately do not follow posted order.
        var start = new DateTime(2026, 11, 1, 7, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 11, 2, 8, 0, 0, DateTimeKind.Utc);
        var late = Content(context); late.PostedOn = end.AddMilliseconds(-1);
        var before = Content(context); before.PostedOn = start.AddMilliseconds(-1);
        var atStart = Content(context); atStart.PostedOn = start;
        var after = Content(context); after.PostedOn = end;
        var missing = Content(context); missing.PostedOn = null;
        var recent = Content(context); recent.PostedOn = start;
        context.SaveChanges();
        await context.Contents.Where(x => x.Id != recent.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedOn, DateTime.UtcNow.AddHours(-1)));
        context.ChangeTracker.Clear();
        var service = Scans(context, grace: 300, batch: 50);
        var date = new DateOnly(2026, 11, 1);
        var job = await service.StartAsync(new("postgres", "content", date, date), default);
        await service.ProcessBatchAsync(default);
        var ids = await context.OrphanScanResults.OrderBy(x => x.ContentId).Select(x => x.ContentId).ToArrayAsync();
        Assert.Equal(new[] { late.Id, atStart.Id }, ids);
        var status = await service.LatestAsync(default);
        Assert.Equal(2, status!.Scanned);
        Assert.Equal(date, status.StartPostedOn);
        Assert.Equal(date, status.EndPostedOn);
    }

    [DatabaseFact]
    public async Task PostedDateRangeRejectsReversedDatesWithoutCreatingJob()
    {
        await using var context = Context();
        await Assert.ThrowsAsync<BadRequestException>(() => Scans(context).StartAsync(
            new("postgres", "content", new DateOnly(2026, 10, 25), new DateOnly(2026, 10, 1)), default));
        Assert.Empty(await context.OrphanScans.ToArrayAsync());
    }

    [DatabaseFact]
    public async Task ElasticsearchPostedDateFilterUsesPacificBoundsOnDiscoveryAndConfirmation()
    {
        var requests = new List<JsonElement>();
        object Respond(RequestData r)
        {
            if (r.Uri.AbsolutePath.EndsWith("/_pit") && r.Method == Elasticsearch.Net.HttpMethod.POST) return new { id = "snapshot" };
            if (r.Method == Elasticsearch.Net.HttpMethod.DELETE) return new { succeeded = true };
            using var stream = new MemoryStream();
            r.PostData!.Write(stream, r.ConnectionSettings);
            var body = JsonDocument.Parse(stream.ToArray()).RootElement.Clone();
            requests.Add(body);
            return body.TryGetProperty("pit", out _) ? Hits(new { _id = "9001", sort = new[] { 41 } }) : Hits();
        }
        var day = new DateOnly(2026, 3, 8); // 23-hour Pacific day.
        await using (var context = Context()) await Scans(context).StartAsync(new("elasticsearch", "content", day, day), default);
        for (var i = 0; i < 2; i++)
        {
            await using var context = Context();
            await Scans(context, Respond).ProcessBatchAsync(default);
        }
        Assert.Equal(2, requests.Count);
        foreach (var request in requests)
        {
            var range = request.GetProperty("query").GetProperty("bool").GetProperty("filter")[1].GetProperty("range").GetProperty("postedOn");
            Assert.Equal(new DateTime(2026, 3, 8, 8, 0, 0, DateTimeKind.Utc), range.GetProperty("gte").GetDateTime());
            Assert.Equal(new DateTime(2026, 3, 9, 7, 0, 0, DateTimeKind.Utc), range.GetProperty("lt").GetDateTime());
            Assert.False(range.TryGetProperty("lte", out _));
        }
    }

    [DatabaseFact]
    public async Task PostgreSqlPostedDateRangeAllowsEitherBoundToBeOmitted()
    {
        await using var setup = Context();
        var before = Content(setup); before.PostedOn = new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc);
        var during = Content(setup); during.PostedOn = new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc);
        var after = Content(setup); after.PostedOn = new DateTime(2026, 10, 26, 12, 0, 0, DateTimeKind.Utc);
        var missing = Content(setup); missing.PostedOn = null;
        setup.SaveChanges();
        await AgeAsync(setup);
        var day = new DateOnly(2026, 10, 25);
        foreach (var (start, end, expected) in new[] {
            ((DateOnly?)day, (DateOnly?)null, new[] { during.Id, after.Id }),
            ((DateOnly?)null, (DateOnly?)day, new[] { before.Id, during.Id }),
            ((DateOnly?)null, (DateOnly?)null, new[] { before.Id, during.Id, after.Id, missing.Id }) })
        {
            await using var context = Context();
            var service = Scans(context, batch: 50);
            var job = await service.StartAsync(new("postgres", "content", start, end), default);
            await service.ProcessBatchAsync(default);
            Assert.Equal(expected, await context.OrphanScanResults.Where(x => x.ScanId == job.Id).OrderBy(x => x.ContentId).Select(x => x.ContentId).ToArrayAsync());
            await service.ProcessBatchAsync(default);
            Assert.Equal("Completed", (await service.LatestAsync(default))!.Status);
        }
    }

    [DatabaseFact]
    public async Task PublishedScanExcludesPrivateAndDraftContent()
    {
        await using var context = Context();
        var published = Content(context);
        Content(context, isPrivate: true);
        Content(context, status: ContentStatus.Draft);
        await AgeAsync(context);
        var service = Scans(context);
        await service.StartAsync(new("postgres", "published"), default);
        await service.ProcessBatchAsync(default);
        var candidate = Assert.Single(await context.OrphanScanResults.ToArrayAsync());
        Assert.Equal(published.Id, candidate.ContentId);
    }

    [DatabaseFact]
    public async Task MigrationCreatesDurableTablesWithOneActiveJobConstraint()
    {
        await using var context = Context();
        var item = Content(context);
        await ApplyMigrationAsync(context, new TNO.DAL.Migrations._1513().DownOperations);
        await ApplyMigrationAsync(context, new TNO.DAL.Migrations._1513().UpOperations);
        Assert.True(await context.Contents.AnyAsync(c => c.Id == item.Id));
        Assert.True(await context.Database.SqlQueryRaw<bool>(
            "SELECT indisvalid AS \"Value\" FROM pg_index WHERE indexrelid = '\"IX_content_posted_on_id\"'::regclass").SingleAsync());
        context.OrphanScans.Add(new OrphanScan());
        await context.SaveChangesAsync();
        context.OrphanScans.Add(new OrphanScan { Status = "Confirming" });
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [DatabaseFact]
    public async Task FailedDispatchDoesNotMarkResultAsQueued()
    {
        await using var context = Context();
        var item = Content(context);
        var job = new OrphanScan { Side = "postgres", Status = "Completed", CompletedOn = DateTime.UtcNow };
        context.OrphanScans.Add(job);
        context.OrphanScanResults.Add(new OrphanScanResult { ScanId = job.Id, ContentId = item.Id, Confirmed = true, DetectedOn = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var sender = new Mock<IIndexRequestSender>();
        sender.Setup(x => x.SendAsync(context)).ThrowsAsync(new InvalidOperationException("Kafka unavailable"));
        var (repair, _) = Service(context, []);
        var controller = new TNO.API.Areas.Admin.Controllers.OrphanedContentController(repair, Scans(context), sender.Object, context);
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Index(item.Id, "content", job.Id));
        await using var verify = Context();
        Assert.Equal("", (await verify.OrphanScanResults.SingleAsync()).Resolution);
    }

    [DatabaseFact]
    public async Task PostedDateCursorResumesAcrossBatchesWhenIdsDoNotFollowPostedOrder()
    {
        var day = new DateOnly(2026, 10, 25);
        long[] expected;
        Guid jobId;
        await using (var context = Context())
        {
            var late = Content(context); late.PostedOn = new DateTime(2026, 10, 26, 6, 0, 0, DateTimeKind.Utc);
            var early = Content(context); early.PostedOn = new DateTime(2026, 10, 25, 7, 0, 0, DateTimeKind.Utc);
            var sameTime = Content(context); sameTime.PostedOn = early.PostedOn;
            context.SaveChanges();
            await AgeAsync(context);
            expected = new[] { early.Id, sameTime.Id, late.Id };
            jobId = (await Scans(context, batch: 1).StartAsync(new("postgres", "content", day, day), default)).Id;
        }
        foreach (var id in expected)
        {
            await using var context = Context();
            await Scans(context, batch: 1).ProcessBatchAsync(default);
            var job = await context.OrphanScans.SingleAsync();
            Assert.Equal(id, job.After);
            Assert.NotNull(job.AfterPostedOn);
        }
        await using var verify = Context();
        await Scans(verify, batch: 1).ProcessBatchAsync(default);
        Assert.Equal(3, (await Scans(verify).LatestAsync(default))!.Scanned);
        Assert.Equal(3, await verify.OrphanScanResults.CountAsync(x => x.ScanId == jobId));
    }

}
