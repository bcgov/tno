using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TNO.AI;

namespace TNO.Test.AI;

/// <summary>
/// Tests for how the direct client reads provider responses.
/// </summary>
public class LlmDirectClientTest
{
    #region Helpers
    private static readonly LlmEndpoint Endpoint = new(new Uri("https://example.test/openai/deployments/x/chat/completions"), "key", "x");

    private class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public int Calls { get; private set; }

        public StubHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.Calls++;
            return Task.FromResult(new HttpResponseMessage(_status) { Content = new StringContent(_body) });
        }
    }

    private static (LlmDirectClient Client, StubHandler Handler) Client(HttpStatusCode status, string body)
    {
        var handler = new StubHandler(status, body);
        return (new LlmDirectClient(new HttpClient(handler), NullLogger.Instance), handler);
    }
    #endregion

    [Fact]
    public async Task FilteredCompletionIsReportedAsContentFiltered()
    {
        var (client, _) = Client(HttpStatusCode.OK, "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":null},\"finish_reason\":\"content_filter\"}]}");

        var result = await client.InvokeAsync(Endpoint, new[] { ("user", "hi") }, true, 3);

        result.IsContentFiltered.Should().BeTrue();
        result.IsTruncated.Should().BeFalse();
        result.Content.Should().BeEmpty();
    }

    [Fact]
    public async Task FilteredPromptThrowsContentFilterException()
    {
        var (client, handler) = Client(HttpStatusCode.BadRequest,
            "{\"error\":{\"message\":\"The response was filtered due to the prompt triggering Azure OpenAI's content management policy.\",\"param\":\"prompt\",\"code\":\"content_filter\",\"status\":400,\"innererror\":{\"code\":\"ResponsibleAIPolicyViolation\"}}}");

        var act = () => client.InvokeAsync(Endpoint, new[] { ("user", "hi") }, true, 3);

        await act.Should().ThrowAsync<LlmContentFilterException>();
        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task RefusalIsCaptured()
    {
        var (client, _) = Client(HttpStatusCode.OK, "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":null,\"refusal\":\"I can't help with that.\"},\"finish_reason\":\"stop\"}]}");

        var result = await client.InvokeAsync(Endpoint, new[] { ("user", "hi") }, true, 3);

        result.Refusal.Should().Be("I can't help with that.");
        result.Content.Should().BeEmpty();
    }
}
