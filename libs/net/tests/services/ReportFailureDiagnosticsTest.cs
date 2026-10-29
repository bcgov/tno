using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TNO.Core.Extensions;
using TNO.Core.Exceptions;
using TNO.Core.Http;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Ches.Models;
using TNO.Services;
using TNO.Services.Config;
using TNO.Services.Managers;

namespace TNO.Test.Services;

public class ReportFailureDiagnosticsTest
{
    [Theory]
    [InlineData("")]
    [InlineData("Report: café — 中文 😀\r\n<html>content</html>")]
    [InlineData("\uFFFD")]
    public void ValidReportTextIsNotCopied(string input)
    {
        input.RemoveInvalidUtf8Characters().Should().BeSameAs(input);
    }

    [Fact]
    public void InvalidSurrogatesMatchThePreviousUtf8RoundTrip()
    {
        var samples = new[] { "\ud800", "\udc00", "a\ud800b", "\ud800\ud800\udc00\udc00", "😀\ud800end", "\udc00😀" };
        foreach (var input in samples)
            input.RemoveInvalidUtf8Characters().Should().Be(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(input)));
    }

    [Fact]
    public void LargeValidReportCleanupDoesNotAllocateReportSizedBuffers()
    {
        var body = new string('x', 2_000_000);
        _ = "warm up".RemoveInvalidUtf8Characters();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var result = body.RemoveInvalidUtf8Characters();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        result.Should().BeSameAs(body);
        allocated.Should().BeLessThan(1024);
    }

    [Fact]
    public void DiagnosticsRetainTheRootCauseAndReportContextWithoutExceptionData()
    {
        var error = new InvalidOperationException("Failed to generate output. ReportId:831, InstanceId:65989",
            new OutOfMemoryException("Insufficient memory while rendering the report."));
        error.Data["body"] = "entire report payload";

        var summary = error.GetDiagnosticSummary();

        summary.Should().Contain("OutOfMemoryException").And.Contain("Insufficient memory")
            .And.Contain("ReportId:831, InstanceId:65989").And.NotContain("entire report payload");
    }

    [Fact]
    public void DiagnosticsAreBoundedEvenWhenAnExceptionContainsAReport()
    {
        var error = new InvalidOperationException(new string('x', 100_000),
            new Exception("Rendering failed: " + new string('y', 100_000)));

        var summary = error.GetDiagnosticSummary();

        summary.Length.Should().BeLessThanOrEqualTo(6000);
        summary.Should().Contain("Rendering failed").And.Contain("[truncated]");
    }

    [Fact]
    public async Task FailureNotificationsIncludeTheEncodedRootCause()
    {
        var ches = new Mock<IChesService>();
        using var http = new HttpClient();
        var api = Mock.Of<IApiService>(a => a.OpenClient.Client == http);
        var manager = new TestServiceManager(api, ches.Object);
        var error = new InvalidOperationException("ReportId:831, InstanceId:65989",
            new OutOfMemoryException("Cannot allocate <report> buffer."));

        await manager.SendErrorEmailAsync("Report failed", error);

        ches.Verify(c => c.SendEmailAsync(It.Is<IEmail>(email =>
            email.Body.Contains("OutOfMemoryException") &&
            email.Body.Contains("&lt;report&gt;") &&
            email.Body.Contains("ReportId:831, InstanceId:65989"))), Times.Once);
    }

    [Fact]
    public async Task DeserializationFailuresLogRequestMetadataWithoutTheResponseBody()
    {
        var logger = new RecordingLogger<HttpRequestClient>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
        using var client = new HttpRequestClient(factory.Object, Mock.Of<IOptionsMonitor<JsonSerializerOptions>>(), logger);
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.test/services/report/instances/65989"),
            Content = new StringContent("{\"id\":false,\"body\":\"" + new string('x', 100_000) + "report-payload\"}", Encoding.UTF8, "application/json"),
        };

        var run = () => client.DeserializeAsync<Dictionary<string, int>>(response);
        await run.Should().ThrowAsync<JsonException>();

        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.Should().Contain("/services/report/instances/65989").And.Contain("$.id")
            .And.NotContain("report-payload");
        entry.Length.Should().BeLessThan(4000);
    }

    [Fact]
    public async Task ApiRetriesDoNotReadTheResponseBodyForLogging()
    {
        var logger = new RecordingLogger<IApiService>();
        using var http = new HttpClient();
        var client = new Mock<IOpenIdConnectRequestClient>();
        client.SetupGet(c => c.Client).Returns(http);
        var api = new TestApiService(client.Object, logger);
        using var body = new UnreadableContent();
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Put, "https://example.test/services/report/instances/65989"),
            Content = body,
        };
        var calls = 0;

        var result = await api.Retry(() => ++calls == 1
            ? Task.FromException<int>(new HttpClientRequestException(response)) : Task.FromResult(42));

        result.Should().Be(42);
        calls.Should().Be(2);
        body.ReadAttempts.Should().Be(0);
        logger.Entries.Should().ContainSingle().Which.Should().Contain("Retry attempt 1").And.Contain("/services/report/instances/65989");
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Entries { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add(formatter(state, exception) + " " + exception?.Message);
    }

    private sealed class TestServiceManager : ServiceManager<ServiceOptions>
    {
        public TestServiceManager(IApiService api, IChesService ches)
            : base(api, ches, Microsoft.Extensions.Options.Options.Create(new ChesOptions() { From = "test@example.test" }),
                Microsoft.Extensions.Options.Options.Create(new ServiceOptions() { SendEmailOnFailure = true, EmailTo = "test@example.test" }),
                NullLogger<ServiceManager<ServiceOptions>>.Instance)
        { }

        public override Task RunAsync() => Task.CompletedTask;
    }

    private sealed class TestApiService : ApiService
    {
        public TestApiService(IOpenIdConnectRequestClient client, ILogger<IApiService> logger)
            : base(client, Microsoft.Extensions.Options.Options.Create(new JsonSerializerOptions()),
                Microsoft.Extensions.Options.Options.Create(new ServiceOptions() { RetryLimit = 2, RetryDelayMS = 0 }), logger)
        { }

        public Task<int> Retry(Func<Task<int>> callback) => RetryRequestAsync(callback);
    }

    private sealed class UnreadableContent : HttpContent
    {
        public int ReadAttempts { get; private set; }
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            ReadAttempts++;
            throw new IOException("Response body must not be read for logging.");
        }
    }
}
