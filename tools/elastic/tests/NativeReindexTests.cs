using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nest;
using TNO.Elastic.Migration;
using Xunit;

namespace TNO.Elastic.Tests;

// These tests create only uniquely named indexes in an explicitly configured disposable cluster.
public sealed class ElasticFactAttribute : FactAttribute
{
    public ElasticFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("MMI_ELASTIC_TEST_URL") == null)
            Skip = "Set MMI_ELASTIC_TEST_URL to a disposable local Elasticsearch 7.17 or 8.x cluster.";
    }
}

public sealed class MigrationFactAttribute : FactAttribute
{
    public MigrationFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("MMI_ELASTIC_TEST_URL") == null ||
            Environment.GetEnvironmentVariable("MMI_MIGRATION_TEST_POSTGRES") == null)
            Skip = "Requires disposable local Elasticsearch and PostgreSQL endpoints.";
    }
}

public sealed class NativeReindexTests : IAsyncLifetime
{
    private readonly string _prefix = "mmi-native-test-" + Guid.NewGuid().ToString("N");
    private readonly HttpClient _http;
    private readonly NativeReindex _copy;
    private string Source => _prefix + "-source";
    private string Target => _prefix + "-target";

    public NativeReindexTests()
    {
        var url = new Uri(Environment.GetEnvironmentVariable("MMI_ELASTIC_TEST_URL") ?? "http://localhost:19274");
        if (!url.IsLoopback) throw new InvalidOperationException("Tests require a disposable loopback Elasticsearch endpoint.");
        _http = new HttpClient { BaseAddress = url };
        var builder = new MigrationBuilder(new ElasticClient(new ConnectionSettings(url).EnableApiVersioningHeader().ThrowExceptions()),
            new global::Elastic.Clients.Elasticsearch.ElasticsearchClient(url),
            Options.Create(new ElasticMigrationOptions { ReindexDelay = 1 }),
            Options.Create(new JsonSerializerOptions()), NullLogger<MigrationBuilder>.Instance);
        _copy = new NativeReindex(builder);
    }

    private async Task<JsonNode> Request(HttpMethod method, string path, JsonNode? body = null)
    {
        var response = await _http.SendAsync(new HttpRequestMessage(method, path)
        {
            Content = body == null ? null : JsonContent.Create(body),
        });
        var text = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, text);
        return JsonNode.Parse(text)!;
    }

    public async Task InitializeAsync()
    {
        await Request(HttpMethod.Put, Source, JsonNode.Parse("""{"settings":{"number_of_replicas":0},"mappings":{"properties":{"id":{"type":"long"}}}}"""));
        await Request(HttpMethod.Put, Target, JsonNode.Parse("""{"settings":{"number_of_replicas":0},"mappings":{"_meta":{"owner":"tno-native-1.0.11"},"properties":{"id":{"type":"long"}}}}"""));
    }

    public async Task DisposeAsync()
    {
        var indexes = await Request(HttpMethod.Get, $"_cat/indices/{_prefix}*?format=json&h=index");
        foreach (var index in indexes.AsArray())
            await Request(HttpMethod.Delete, index!["index"]!.GetValue<string>());
        _http.Dispose();
    }

    private Task<JsonNode> Put(int id, string json, string? index = null, string extra = "") =>
        Request(HttpMethod.Put, $"{index ?? Source}/_doc/{id}?refresh=true{extra}", JsonNode.Parse(json));

    [ElasticFact]
    public async Task CopiesLegacyAndCurrentRevisionsAndSanitizesPublishedTranscripts()
    {
        await Put(1, """{"id":1,"contentType":0,"isApproved":false,"body":"private transcript","analysis":{"summary":"private"},"topics":[{"id":1}]}""");
        await Put(2, """{"id":2,"projectionRevision":7,"contentType":0,"isApproved":true,"body":"approved","topics":[{"isSystem":true}]}""");
        await _copy.CopyAsync(Source, Target, true, true);
        var legacy = await Request(HttpMethod.Get, $"{Target}/_doc/1");
        Assert.Equal(0, legacy["_version"]!.GetValue<long>());
        Assert.Equal(0, legacy["_source"]!["projectionRevision"]!.GetValue<long>());
        Assert.Equal("", legacy["_source"]!["body"]!.GetValue<string>());
        Assert.Null(legacy["_source"]!["analysis"]);
        Assert.False(legacy["_source"]!["topics"]![0]!["isSystem"]!.GetValue<bool>());
        var current = await Request(HttpMethod.Get, $"{Target}/_doc/2");
        Assert.Equal(7, current["_version"]!.GetValue<long>());
        Assert.Equal("approved", current["_source"]!["body"]!.GetValue<string>());
        // The next normal external-version application write must succeed, including legacy 0 -> 1.
        await Put(1, """{"id":1,"projectionRevision":1} """, Target, "&version_type=external_gte&version=1");
    }

    [ElasticFact]
    public async Task KeepsNewerRepairsAndSkipsCompletedCopiesOnRestart()
    {
        await Put(1, """{"id":1,"projectionRevision":3,"body":"old"}""");
        await Put(1, """{"id":1,"projectionRevision":9,"body":"repaired"}""", Target, "&version_type=external_gte&version=9");
        await _copy.CopyAsync(Source, Target, false, true);
        await _copy.CopyAsync(Source, Target, false, true);
        var result = await Request(HttpMethod.Get, $"{Target}/_doc/1");
        Assert.Equal("repaired", result["_source"]!["body"]!.GetValue<string>());
        Assert.Equal(9, result["_version"]!.GetValue<long>());
    }

    private async Task<JsonObject> State()
    {
        var source = await Request(HttpMethod.Get, $"{Source}/_settings");
        return new JsonObject
        {
            ["owner"] = "tno-native-1.0.11", ["source"] = Source,
            ["sourceUuid"] = source[Source]!["settings"]!["index"]!["uuid"]!.DeepClone(),
        };
    }

    private Task<JsonNode> Save(JsonObject state) => Request(HttpMethod.Put, $"{Target}/_mapping", new JsonObject { ["_meta"] = state });

    [ElasticFact]
    public async Task ReconnectsToTaskLaunchedBeforeCoordinatorRestart()
    {
        await Put(1, """{"id":1,"body":"original"}""");
        var task = await Request(HttpMethod.Post, "_reindex?wait_for_completion=false", NativeReindex.CreateRequest(Source, Target, false, true));
        var state = await State();
        state["task"] = task["task"]!.DeepClone();
        await Save(state);
        // Wait for this small server-side task, then change source without launching a new copy.
        for (var i = 0; i < 50; i++)
        {
            var result = await Request(HttpMethod.Get, $"_tasks/{task["task"]!.GetValue<string>()}");
            if (result["completed"]?.GetValue<bool>() == true) break;
            await Task.Delay(100);
        }
        await Put(1, """{"id":1,"body":"changed"}""");
        await _copy.CopyAsync(Source, Target, false, true);
        var doc = await Request(HttpMethod.Get, $"{Target}/_doc/1");
        Assert.Equal("original", doc["_source"]!["body"]!.GetValue<string>());
    }

    [ElasticFact]
    public async Task RecoversTaskWhenCoordinatorLostTheLaunchResponse()
    {
        for (var id = 1; id <= 4; id++)
            await Put(id, new JsonObject { ["id"] = id }.ToJsonString());
        var body = NativeReindex.CreateRequest(Source, Target, false, true);
        body["source"]!["size"] = 1;
        using var request = new HttpRequestMessage(HttpMethod.Post, "_reindex?wait_for_completion=false&requests_per_second=1")
        { Content = JsonContent.Create(body) };
        request.Headers.Add("X-Opaque-Id", $"tno-native-1.0.11:{Target}");
        var launched = await _http.SendAsync(request);
        launched.EnsureSuccessStatusCode();
        var task = JsonNode.Parse(await launched.Content.ReadAsStringAsync())!["task"]!.GetValue<string>();
        // Destination has only its owner marker, exactly as if the POST response was lost.
        await _copy.CopyAsync(Source, Target, false, true);
        var mapping = await Request(HttpMethod.Get, $"{Target}/_mapping");
        Assert.Equal(task, mapping[Target]!["mappings"]!["_meta"]!["task"]!.GetValue<string>());
    }

    [ElasticFact]
    public async Task MissingTaskAllowsReplayWithoutDiscardingDestination()
    {
        await Put(1, """{"id":1,"projectionRevision":2}""");
        var state = await State();
        state["task"] = "missing-node:123";
        await Save(state);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _copy.CopyAsync(Source, Target, false, true));
        await _copy.CopyAsync(Source, Target, false, true);
        var doc = await Request(HttpMethod.Get, $"{Target}/_doc/1");
        Assert.Equal(2, doc["_version"]!.GetValue<long>());
    }

    [ElasticFact]
    public async Task FailedCopyIsNotMarkedCompleteAndCanBeRetried()
    {
        await Request(HttpMethod.Put, $"{Target}/_mapping", JsonNode.Parse("""{"properties":{"number":{"type":"long"}}}"""));
        await Put(1, """{"id":1,"number":"bad"}""");
        await Assert.ThrowsAsync<InvalidOperationException>(() => _copy.CopyAsync(Source, Target, false, true));
        var mapping = await Request(HttpMethod.Get, $"{Target}/_mapping");
        Assert.Null(mapping[Target]!["mappings"]!["_meta"]!["complete"]);
        await Put(1, """{"id":1,"number":123}""");
        await _copy.CopyAsync(Source, Target, false, true);
    }

    [ElasticFact]
    public async Task RefusesReplacedSourceAndUnownedDestination()
    {
        var state = await State();
        state["sourceUuid"] = "wrong";
        await Save(state);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _copy.CopyAsync(Source, Target, false, true));
        await Save(new JsonObject { ["owner"] = "someone-else" });
        await Assert.ThrowsAsync<InvalidOperationException>(() => _copy.CopyAsync(Source, Target, false, true));
    }

    [ElasticFact]
    public async Task RollbackRemovesAnalysisFieldsButKeepsRevisionOrdering()
    {
        await Put(1, """{"id":1,"projectionRevision":4,"body":"text","analysis":{"summary":"old"},"topics":[{"isSystem":true}]}""");
        await _copy.CopyAsync(Source, Target, false, false);
        var doc = await Request(HttpMethod.Get, $"{Target}/_doc/1");
        Assert.Equal(4, doc["_version"]!.GetValue<long>());
        Assert.Null(doc["_source"]!["analysis"]);
        Assert.Null(doc["_source"]!["topics"]![0]!["isSystem"]);
        Assert.Equal("text", doc["_source"]!["body"]!.GetValue<string>());
    }
    [MigrationFact]
    public async Task RepairsDatabaseDifferencesBuildsEvidenceAndSwitchesConcreteIndexesAtomically()
    {
        var cs = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("MMI_MIGRATION_TEST_POSTGRES"));
        Assert.Contains(cs.Host, new[] { "127.0.0.1", "localhost" });
        var database = "mmi_migration_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(cs.ConnectionString);
        await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE DATABASE {database}", admin)) await command.ExecuteNonQueryAsync();
        try
        {
            cs.Database = database;
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(cs.ConnectionString);
            dataSourceBuilder.EnableDynamicJson();
            await using var dataSource = dataSourceBuilder.Build();
            await using var context = new TNOContext(new DbContextOptionsBuilder<TNOContext>().UseNpgsql(dataSource).Options);
            await context.Database.EnsureCreatedAsync();
            var license = new TNO.Entities.License("test", 0);
            var media = new MediaType("test");
            var source = new Source("Test", "TEST", license);
            var legacy = new Content("legacy", "legacy", source, ContentType.PrintContent, license, media) { Status = ContentStatus.Published, Body = "legacy" };
            var stale = new Content("stale", "stale", source, ContentType.AudioVideo, license, media) { Status = ContentStatus.Published, Body = "private transcript", IsApproved = false };
            var missing = new Content("missing", "missing", source, ContentType.PrintContent, license, media) { Status = ContentStatus.Draft, Body = "missing" };
            context.Contents.AddRange(legacy, stale, missing);
            await context.SaveChangesAsync();
            context.ContentAnalyses.Add(new ContentAnalysis(stale.Id, "hash") { IsCurrent = true, Summary = "analysis" });
            await context.SaveChangesAsync();
            await context.Database.ExecuteSqlRawAsync("UPDATE content SET projection_revision = CASE WHEN uid = 'legacy' THEN 0 ELSE 3 END");
            context.ChangeTracker.Clear();

            var published = _prefix + "-published";
            await Request(HttpMethod.Put, published, JsonNode.Parse("""{"settings":{"number_of_replicas":0},"mappings":{"properties":{"id":{"type":"long"}}}}"""));
            foreach (var index in new[] { Source, published })
            {
                await Request(HttpMethod.Put, $"{index}/_doc/{legacy.Id}?refresh=true", new JsonObject { ["id"] = legacy.Id, ["contentType"] = 1, ["body"] = "legacy" });
                await Request(HttpMethod.Put, $"{index}/_doc/{stale.Id}?refresh=true", new JsonObject { ["id"] = stale.Id, ["projectionRevision"] = 1, ["contentType"] = 0, ["body"] = "old transcript" });
                await Request(HttpMethod.Put, $"{index}/_doc/999999?refresh=true", new JsonObject { ["id"] = 999999 });
            }
            await Request(HttpMethod.Put, $"{published}/_doc/{missing.Id}?refresh=true", new JsonObject { ["id"] = missing.Id });
            var options = new ElasticMigrationOptions
            {
                ContentIndex = Source, PublishedIndex = published, EvidenceIndex = _prefix + "-evidence",
                MigrationIndex = _prefix + "-history",
                MigrationsPath = Path.Combine(AppContext.BaseDirectory, "Migrations"), ReindexDelay = 1,
            };
            var serializer = Options.Create(new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
            });
            var url = _http.BaseAddress!;
            var builder = new MigrationBuilder(new ElasticClient(new ConnectionSettings(url).EnableApiVersioningHeader().ThrowExceptions()),
                new TNO.Elastic.Migration.TNOElasticClient(Options.Create<TNO.Elastic.ElasticOptions>(new TNO.Elastic.ElasticOptions { Url = url }), serializer), Options.Create(options),
                serializer, NullLogger<MigrationBuilder>.Instance);
            using var services = new ServiceCollection().BuildServiceProvider();
            var analysis = new ContentAnalysisService(context, new ClaimsPrincipal(), services, NullLogger<ContentAnalysisService>.Instance);
            var migration = new Migration_1011(builder, null!, analysis, context, serializer);
            foreach (var retained in new[] { Source, published, options.EvidenceIndex })
                await Request(HttpMethod.Put, retained + "_v1.0.11", JsonNode.Parse("""{"settings":{"number_of_replicas":0}}"""));
            await migration.RunUpAsync();
            var topology = await Request(HttpMethod.Get, $"{Source}/_settings");
            Assert.Equal("0", topology.AsObject().First().Value!["settings"]!["index"]!["number_of_replicas"]!.GetValue<string>());
            // Replaying the coordinator after cutover must not recreate indexes or duplicate history.
            await migration.RunUpAsync();
            var alias = await Request(HttpMethod.Get, $"_alias/{Source}");
            var firstUpgrade = alias.AsObject().Single().Key;
            Assert.StartsWith(Source + "_v1.0.11-", firstUpgrade);
            Assert.Equal(3, (await Request(HttpMethod.Get, $"{Source}/_count"))["count"]!.GetValue<int>());
            Assert.Equal(2, (await Request(HttpMethod.Get, $"{published}/_count"))["count"]!.GetValue<int>());
            Assert.Equal(1, (await Request(HttpMethod.Get, $"{options.EvidenceIndex}/_count"))["count"]!.GetValue<int>());
            Assert.Equal(1, (await Request(HttpMethod.Get, $"{options.MigrationIndex}/_count"))["count"]!.GetValue<int>());
            var repaired = await Request(HttpMethod.Get, $"{Source}/_doc/{missing.Id}");
            Assert.Equal(3, repaired["_version"]!.GetValue<long>());
            var protectedDoc = await Request(HttpMethod.Get, $"{published}/_doc/{stale.Id}");
            Assert.Equal("", protectedDoc["_source"]!["body"]!.GetValue<string>());
            Assert.Null(protectedDoc["_source"]!["analysis"]);
            await migration.RunDownAsync();
            alias = await Request(HttpMethod.Get, $"_alias/{Source}");
            var firstRollback = alias.AsObject().Single().Key;
            Assert.StartsWith(Source + "_v1.0.10-rollback-from-1.0.11-", firstRollback);
            await Request(HttpMethod.Post, options.MigrationIndex + "/_refresh");
            Assert.Equal(0, (await Request(HttpMethod.Get, $"{options.MigrationIndex}/_count"))["count"]!.GetValue<int>());
            Assert.Equal(System.Net.HttpStatusCode.NotFound, (await _http.GetAsync($"_alias/{options.EvidenceIndex}")).StatusCode);
            await migration.RunDownAsync();
            Assert.NotNull((await Request(HttpMethod.Get, $"_alias/{Source}"))[firstRollback]);

            // A new cycle must preserve both backups and copy changes made after rollback.
            await context.Database.ExecuteSqlRawAsync("UPDATE content SET headline = 'updated after rollback', projection_revision = 4 WHERE uid = 'missing'");
            await migration.RunUpAsync();
            var secondUpgrade = (await Request(HttpMethod.Get, $"_alias/{Source}")).AsObject().Single().Key;
            Assert.NotEqual(firstUpgrade, secondUpgrade);
            Assert.Equal(4, (await Request(HttpMethod.Get, $"{Source}/_doc/{missing.Id}"))["_version"]!.GetValue<long>());
            await migration.RunUpAsync();
            Assert.NotNull((await Request(HttpMethod.Get, $"_alias/{Source}"))[secondUpgrade]);
            Assert.Equal(1, (await Request(HttpMethod.Get, $"{options.EvidenceIndex}/_count"))["count"]!.GetValue<int>());
            await migration.RunDownAsync();
            var secondRollback = (await Request(HttpMethod.Get, $"_alias/{Source}")).AsObject().Single().Key;
            Assert.NotEqual(firstRollback, secondRollback);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, (await _http.GetAsync($"_alias/{options.EvidenceIndex}")).StatusCode);
            foreach (var retained in new[] { firstUpgrade, firstRollback, secondUpgrade })
                Assert.Equal(3, (await Request(HttpMethod.Get, $"{retained}/_count"))["count"]!.GetValue<int>());
            Assert.Equal(0, (await Request(HttpMethod.Get, $"{options.MigrationIndex}/_count"))["count"]!.GetValue<int>());

        }
        finally
        {
            await using var command = new NpgsqlCommand($"DROP DATABASE {database} WITH (FORCE)", admin);
            await command.ExecuteNonQueryAsync();
        }
    }

}
