using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TNO.Core.Http;
using TNO.Elastic;

namespace TNO.Test.DAL.Elastic;

/// <summary>
/// A local HTTP listener that stands in for Elasticsearch, so TNOElasticClient and its callers can be tested
/// against real HTTP (status codes, headers, bodies) without a cluster.
/// </summary>
public sealed class FakeElasticServer : IDisposable
{
    /// <summary>A request the server received.</summary>
    public sealed record Request(string Method, string PathAndQuery, string Body, IReadOnlyDictionary<string, string?> Headers, int ClientPort);

    /// <summary>What to answer.</summary>
    public sealed record Reply(int Status, string Body = "", TimeSpan? Delay = null);

    private readonly HttpListener _listener = new();
    private readonly Func<Request, Reply> _handler;
    private readonly CancellationTokenSource _stop = new();

    /// <summary>get - Every request received, in order.</summary>
    public ConcurrentQueue<Request> Requests { get; } = new();

    /// <summary>get - The server's base URL.</summary>
    public Uri Url { get; }

    public FakeElasticServer(Func<Request, Reply> handler)
    {
        _handler = handler;
        Url = new Uri($"http://127.0.0.1:{FreePort()}/");
        _listener.Prefixes.Add(Url.ToString());
        _listener.Start();
        _ = Task.Run(ServeAsync);
    }

    /// <summary>A port nothing listens on: connecting to it is refused.</summary>
    public static int FreePort()
    {
        var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        var port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }

    /// <summary>A TNOElasticClient pointed at 'url'.</summary>
    public static TNOElasticClient Client(Uri url, string username = "", string password = "", string apiKey = "", string evidenceIndex = "content_evidence")
        => new(new HttpRequestClient(new HttpClients(), null!, NullLogger<HttpRequestClient>.Instance),
            Options.Create(new ElasticOptions { Url = url, Username = username, Password = password, ApiKey = apiKey, EvidenceIndex = evidenceIndex }),
            NullLogger<TNOElasticClient>.Instance);

    private async Task ServeAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            HttpListenerContext context;
            try { context = await _listener.GetContextAsync(); }
            catch (Exception) { return; }
            _ = Task.Run(async () =>
            {
                // The client compresses request bodies.
                Stream input = context.Request.Headers["Content-Encoding"] == "gzip"
                    ? new System.IO.Compression.GZipStream(context.Request.InputStream, System.IO.Compression.CompressionMode.Decompress)
                    : context.Request.InputStream;
                using var reader = new StreamReader(input);
                var body = await reader.ReadToEndAsync();
                var headers = context.Request.Headers.AllKeys.Where(k => k != null).ToDictionary(k => k!, k => context.Request.Headers[k]);
                var request = new Request(context.Request.HttpMethod, context.Request.Url!.PathAndQuery, body, headers, context.Request.RemoteEndPoint.Port);
                Requests.Enqueue(request);
                var reply = _handler(request);
                if (reply.Delay != null)
                {
                    try { await Task.Delay(reply.Delay.Value, _stop.Token); } catch (OperationCanceledException) { }
                }
                try
                {
                    context.Response.StatusCode = reply.Status;
                    context.Response.ContentType = "application/json";
                    // Elasticsearch identifies itself; the client checks this header.
                    context.Response.Headers["X-elastic-product"] = "Elasticsearch";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(reply.Body);
                    if (request.Method != "HEAD") await context.Response.OutputStream.WriteAsync(bytes);
                    context.Response.Close();
                }
                catch (Exception) { }
            });
        }
    }

    public void Dispose()
    {
        _stop.Cancel();
        _listener.Close();
    }

    private sealed class HttpClients : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
