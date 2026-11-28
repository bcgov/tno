using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TNO.Elastic;
using TNO.Elastic.Migration;
using TNO.Elastic.Models;
using Xunit;

namespace TNO.Elastic.Migration.Tests;

/// <summary>
/// MigrationService decides which migrations run from the migrations index, so reading it wrongly reruns
/// migrations against populated clusters. These answer its requests with a fake client; no cluster is needed.
/// </summary>
public class MigrationServiceTests
{
    private sealed class FakeClient(Func<HttpMethod, string, string?, ElasticResponse> respond) : ITNOElasticClient
    {
        public List<(HttpMethod Method, string Path, string? Body)> Requests { get; } = [];

        public Task<ElasticResponse> RequestAsync(HttpMethod method, string path, string? body = null, string? opaqueId = null, CancellationToken cancellationToken = default)
        {
            Requests.Add((method, path, body));
            return Task.FromResult(respond(method, path, body));
        }

        public Task<SearchResultModel<T>> SearchAsync<T>(string index, JsonDocument query) where T : class => throw new NotSupportedException();
        public Task<SearchResultModel<T>> SearchAsync<T>(string index, JsonElement query) where T : class => throw new NotSupportedException();
        public Task<JsonDocument> SearchRawAsync(string index, JsonDocument query) => throw new NotSupportedException();
        public Task<ValidateResultModel> ValidateAsync(string index, JsonDocument query) => throw new NotSupportedException();
    }

    private static MigrationService Service(FakeClient client, ElasticMigrationOptions? options = null)
        => new(Options.Create(options ?? new ElasticMigrationOptions()), client, new ServiceCollection().BuildServiceProvider(), NullLogger<MigrationService>.Instance);

    private static string Latest => Assembly.GetAssembly(typeof(MigrationService))!.GetMigrationTypes()
        .Select(t => t.GetCustomAttribute<MigrationAttribute>()!.Id)
        .OrderBy(v => Version.Parse(v)).Last();

    /// <summary>The records NEST wrote: camelCase 'version' and 'description'.</summary>
    private static string History(params string[] versions) => JsonSerializer.Serialize(new
    {
        hits = new { hits = versions.Select(v => new { _id = v, _source = new { version = v, description = $"Migration {v}" } }) },
    });

    [Fact]
    public async Task ReadsTheVersionNestRecordedAndRunsNothingWhenCurrent()
    {
        // Out of order on purpose: the newest is chosen by version, not by position.
        var client = new FakeClient((method, path, _) => (method.Method, path) switch
        {
            ("HEAD", "migrations") => new ElasticResponse(200, ""),
            ("POST", "migrations/_search") => new ElasticResponse(200, History("1.0.9", Latest, "1.0.10")),
            _ => new ElasticResponse(500, "{\"error\":\"unexpected request\"}"),
        });

        await Service(client).RunAsync(default);

        Assert.All(client.Requests, r => Assert.True(r.Method == HttpMethod.Head || r.Path == "migrations/_search", $"{r.Method} {r.Path}"));
    }

    [Theory]
    [InlineData(403)]
    [InlineData(502)]
    [InlineData(null)]
    public async Task BaselineStopsWhenIndexExistenceIsUnknown(int? status)
    {
        var client = new FakeClient((method, path, _) => (method.Method, path) switch
        {
            ("HEAD", "migrations") => new ElasticResponse(200, ""),
            ("POST", "migrations/_search") => new ElasticResponse(200, History()),
            ("HEAD", _) => new ElasticResponse(status, "", status == null ? new HttpRequestException("connection refused") : null),
            _ => new ElasticResponse(500, "{\"error\":\"unexpected request\"}"),
        });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Service(client).RunAsync(default));

        Assert.Contains("Cannot tell whether index", error.Message);
        Assert.DoesNotContain(client.Requests, r => r.Method != HttpMethod.Head && r.Method != HttpMethod.Post);
    }

    [Fact]
    public async Task BaselineRefusesExistingIndexesWithoutHistory()
    {
        var client = new FakeClient((method, path, _) => (method.Method, path) switch
        {
            ("HEAD", "migrations") => new ElasticResponse(200, ""),
            ("POST", "migrations/_search") => new ElasticResponse(200, History()),
            ("HEAD", "unpublished_content") => new ElasticResponse(200, ""),
            ("HEAD", "content") => new ElasticResponse(404, ""),
            _ => new ElasticResponse(500, "{\"error\":\"unexpected request\"}"),
        });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Service(client).RunAsync(default));

        Assert.Contains("no migration history", error.Message);
        Assert.Contains("unpublished_content", error.Message);
    }

    [Fact]
    public async Task CreatingTheMigrationIndexGivesUpOnAPersistentGateway()
    {
        var client = new FakeClient((_, _, _) => new ElasticResponse(502, "<html>Bad Gateway</html>"));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Service(client, new ElasticMigrationOptions { GatewayRetryLimit = 1 }).RunAsync(default));

        Assert.Contains("HTTP 502", error.Message);
    }

    [Fact]
    public void ACompletedTaskWithFailuresDidNotCopyEverything()
    {
        Assert.False(NativeReindex.TaskFailed(System.Text.Json.Nodes.JsonNode.Parse("{\"completed\":true,\"response\":{\"failures\":[]}}")!));
        Assert.True(NativeReindex.TaskFailed(System.Text.Json.Nodes.JsonNode.Parse("{\"completed\":true,\"response\":{\"failures\":[{\"cause\":{\"type\":\"version_conflict_engine_exception\"}}]}}")!));
        Assert.True(NativeReindex.TaskFailed(System.Text.Json.Nodes.JsonNode.Parse("{\"completed\":true,\"response\":{\"canceled\":\"by user request\",\"failures\":[]}}")!));
        Assert.True(NativeReindex.TaskFailed(System.Text.Json.Nodes.JsonNode.Parse("{\"completed\":true,\"error\":{\"type\":\"exception\"}}")!));
    }
}
