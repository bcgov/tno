using AwesomeAssertions;

namespace TNO.Test.DAL.Elastic;

/// <summary>
/// TNOElasticClient.RequestAsync against a local HTTP listener: what it sends, and how it reports what came back.
/// </summary>
public class TNOElasticClientTest
{
    [Fact]
    public async Task RequestAsync_SendsMethodPathQueryAndBody()
    {
        using var server = new FakeElasticServer(_ => new(200, "{\"ok\":true}"));
        var client = FakeElasticServer.Client(server.Url);

        var response = await client.RequestAsync(HttpMethod.Post, "/content/_search?size=1", "{\"query\":{}}", "trace-1", TestContext.Current.CancellationToken);

        response.Success.Should().BeTrue();
        response.HttpStatusCode.Should().Be(200);
        response.Body.Should().Be("{\"ok\":true}");
        server.Requests.Should().ContainSingle();
        var request = server.Requests.Single();
        request.Method.Should().Be("POST");
        request.PathAndQuery.Should().Be("/content/_search?size=1");
        request.Headers["X-Opaque-Id"].Should().Be("trace-1");
        // 8.19 and 9.x both accept 8; 9.x rejects the 7 that NEST sent.
        request.Headers["Accept"].Should().Contain("compatible-with=8");
    }

    [Fact]
    public async Task RequestAsync_ReturnsErrorsInsteadOfThrowing()
    {
        using var server = new FakeElasticServer(_ => new(404, "{\"error\":{\"type\":\"index_not_found_exception\"},\"status\":404}"));
        var client = FakeElasticServer.Client(server.Url);

        var response = await client.RequestAsync(HttpMethod.Get, "missing/_mapping", cancellationToken: TestContext.Current.CancellationToken);

        response.Success.Should().BeFalse();
        response.NotFound.Should().BeTrue();
        response.Body.Should().Contain("index_not_found_exception");
        response.ToString().Should().StartWith("HTTP 404: ");
    }

    [Fact]
    public async Task RequestAsync_HeadNotFoundHasEmptyBody()
    {
        using var server = new FakeElasticServer(_ => new(404));
        var client = FakeElasticServer.Client(server.Url);

        var response = await client.RequestAsync(HttpMethod.Head, "missing", cancellationToken: TestContext.Current.CancellationToken);

        response.NotFound.Should().BeTrue();
        response.Body.Should().BeEmpty();
        response.ToString().Should().Be("HTTP 404");
    }

    [Fact]
    public async Task RequestAsync_NoResponseHasNoStatus()
    {
        var client = FakeElasticServer.Client(new Uri($"http://127.0.0.1:{FakeElasticServer.FreePort()}/"));

        var response = await client.RequestAsync(HttpMethod.Get, "_cluster/health", cancellationToken: TestContext.Current.CancellationToken);

        response.Success.Should().BeFalse();
        response.HttpStatusCode.Should().BeNull();
        response.ToString().Should().StartWith("no response (");
    }

    [Fact]
    public async Task RequestAsync_ThrowsWhenCancelled()
    {
        using var server = new FakeElasticServer(_ => new(200, "{}", TimeSpan.FromSeconds(30)));
        var client = FakeElasticServer.Client(server.Url);
        using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        var act = () => client.RequestAsync(HttpMethod.Get, "_cluster/health", cancellationToken: cancel.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RequestAsync_SendsBasicAuthentication()
    {
        using var server = new FakeElasticServer(_ => new(200, "{}"));
        var client = FakeElasticServer.Client(server.Url, username: "elastic", password: "secret");

        await client.RequestAsync(HttpMethod.Get, "/", cancellationToken: TestContext.Current.CancellationToken);

        var expected = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("elastic:secret"));
        server.Requests.Single().Headers["Authorization"].Should().Be($"Basic {expected}");
    }

    [Fact]
    public async Task RequestAsync_SendsApiKey()
    {
        using var server = new FakeElasticServer(_ => new(200, "{}"));
        var client = FakeElasticServer.Client(server.Url, apiKey: "key-1");

        await client.RequestAsync(HttpMethod.Get, "/", cancellationToken: TestContext.Current.CancellationToken);

        // Basic authentication wins when the ELASTIC_USERNAME/ELASTIC_PASSWORD fallback is set in this environment.
        var expected = String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELASTIC_USERNAME")) || String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELASTIC_PASSWORD"))
            ? "ApiKey key-1"
            : null;
        if (expected != null) server.Requests.Single().Headers["Authorization"].Should().Be(expected);
    }

    [Fact]
    public async Task Clients_ShareOneConnectionPoolPerCluster()
    {
        using var server = new FakeElasticServer(_ => new(200, "{}"));

        // Many short-lived wrappers, as the API creates one per request scope.
        for (var i = 0; i < 20; i++)
            (await FakeElasticServer.Client(server.Url).RequestAsync(HttpMethod.Get, "/", cancellationToken: TestContext.Current.CancellationToken)).Success.Should().BeTrue();

        // One shared connection pool reuses its keep-alive connection; a client per wrapper opens a connection each.
        server.Requests.Should().HaveCount(20);
        server.Requests.Select(r => r.ClientPort).Distinct().Should().ContainSingle();
    }
}
