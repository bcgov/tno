using System.Net;
using System.Text;
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

    [Fact]
    public async Task RawRequestsRetainTheirBufferLimit()
    {
        using var http = new HttpClient(new Handler(() => new StringContent(new string('x', 1000))))
        { MaxResponseContentBufferSize = 32 };
        await Assert.ThrowsAsync<HttpRequestException>(() => Client(http).GetAsync("http://test/raw"));
    }
}
