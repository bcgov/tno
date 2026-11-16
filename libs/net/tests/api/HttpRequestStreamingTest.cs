using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TNO.Core.Http;
using Xunit;

namespace TNO.Test.API;

public class HttpRequestStreamingTest
{
    private sealed class Handler(Func<HttpContent> content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content(), RequestMessage = request });
    }

    private sealed class StalledStream : MemoryStream
    {
        public bool WasDisposed { get; private set; }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
    }

    private static HttpRequestClient Client(HttpClient http)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(http);
        return new HttpRequestClient(factory.Object, null!, NullLogger<HttpRequestClient>.Instance);
    }

    [Fact]
    public async Task TypedJsonDoesNotBufferTheResponse()
    {
        var text = new string('x', 100_000);
        using var http = new HttpClient(new Handler(() => new StringContent("\"" + text + "\"", Encoding.UTF8, "application/json")))
        { MaxResponseContentBufferSize = 32 };
        Assert.Equal(text, await Client(http).GetAsync<string>("http://test/report"));
    }

    [Fact]
    public async Task TypedJsonBodyReadHasADeadlineAndDisposesTheResponse()
    {
        var stream = new StalledStream();
        using var http = new HttpClient(new Handler(() =>
        {
            var content = new StreamContent(stream);
            content.Headers.ContentType = new("application/json");
            return content;
        }))
        { Timeout = TimeSpan.FromMilliseconds(100) };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Client(http).GetAsync<string>("http://test/report").WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.True(stream.WasDisposed);
    }

    private sealed class ConflictHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = request,
                Content = new StringContent("{\"type\":\"DbUpdateConcurrencyException\"}", Encoding.UTF8, "application/json"),
            });
    }

    [Fact]
    public async Task StreamingErrorsKeepTheBodyAvailableForConcurrencyRecovery()
    {
        using var http = new HttpClient(new ConflictHandler());
        var error = await Assert.ThrowsAsync<TNO.Core.Exceptions.HttpClientRequestException>(() => Client(http).GetAsync<string>("http://test/report"));
        Assert.Contains("DbUpdateConcurrencyException", await error.Response!.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        error.Response.Dispose();
    }

    private sealed class ErrorHandler(HttpStatusCode status, Func<HttpContent> content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(status) { Content = content(), RequestMessage = request });
    }

    /// <summary>
    /// An unbuffered body, as a streamed response has, of the specified 'mediaType'.
    /// </summary>
    private static StreamContent Streamed(string body, string mediaType)
    {
        var content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(body)));
        content.Headers.ContentType = new(mediaType);
        return content;
    }

    [Fact]
    public async Task StreamingErrorsKeepTheirContentType()
    {
        // CHES answers a validation failure with problem+json, and its handler only parses a JSON body.
        var json = "{\"title\":\"Unprocessable\",\"errors\":[{\"message\":\"Invalid email\"}]}";
        using var http = new HttpClient(new ErrorHandler(HttpStatusCode.UnprocessableEntity, () => Streamed(json, "application/problem+json")));
        var error = await Assert.ThrowsAsync<TNO.Core.Exceptions.HttpClientRequestException>(() => Client(http).GetAsync<string>("http://test/email"));
        Assert.Equal("application/problem+json", error.Response!.Content.Headers.ContentType?.MediaType);
        Assert.Equal(json, await error.Response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        error.Response.Dispose();
    }

    [Fact]
    public async Task StreamingErrorsKeepTheWholeBodyAndCapOnlyTheMessage()
    {
        // Outside production the API adds a stack trace, which makes a concurrency error longer than the message cap.
        var json = JsonSerializer.Serialize(new { type = "DbUpdateConcurrencyException", stackTrace = new string('s', 10_000) });
        using var http = new HttpClient(new ErrorHandler(HttpStatusCode.BadRequest, () => Streamed(json, "application/json")));
        var error = await Assert.ThrowsAsync<TNO.Core.Exceptions.HttpClientRequestException>(() => Client(http).GetAsync<string>("http://test/report"));
        var body = await error.Response!.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal(json, body);
        Assert.Equal("DbUpdateConcurrencyException", JsonDocument.Parse(body).RootElement.GetProperty("type").GetString());
        Assert.Contains("...[truncated, ", error.Message);
        Assert.True(error.Message.Length < json.Length);
        error.Response.Dispose();
    }

    [Fact]
    public async Task AnUnreadableErrorBodyStillReportsTheFailure()
    {
        var stream = new StalledStream();
        using var http = new HttpClient(new ErrorHandler(HttpStatusCode.BadGateway, () =>
        {
            var content = new StreamContent(stream);
            content.Headers.ContentType = new("application/json");
            return content;
        }))
        { Timeout = TimeSpan.FromMilliseconds(100) };
        var error = await Assert.ThrowsAsync<TNO.Core.Exceptions.HttpClientRequestException>(() => Client(http).GetAsync<string>("http://test/report").WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.True(stream.WasDisposed);
        Assert.Empty(await error.Response!.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        error.Response.Dispose();
    }

    [Fact]
    public async Task RawRequestsRetainTheirBufferLimit()
    {
        using var http = new HttpClient(new Handler(() => new StringContent(new string('x', 1000))))
        { MaxResponseContentBufferSize = 32 };
        await Assert.ThrowsAsync<HttpRequestException>(() => Client(http).GetAsync("http://test/raw"));
    }
}
