using System.Collections.Concurrent;

namespace TNO.AI;

/// <summary>
/// LlmRateLimiter class, keeps requests to a model deployment within its requests-per-minute and
/// tokens-per-minute limits across the whole process. Each deployment has its own one-minute
/// sliding window; a request waits until it fits.
/// </summary>
public static class LlmRateLimiter
{
    private static readonly ConcurrentDictionary<string, Window> _windows = new();
    private static readonly TimeSpan Period = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Wait until a request of the specified size fits the deployment's limits, then record it.
    /// A request larger than the whole tokens-per-minute limit waits for an empty window and runs alone.
    /// </summary>
    /// <param name="key">Identifies the deployment (endpoint and deployment name).</param>
    /// <param name="requestsPerMinute">Null or zero is unlimited.</param>
    /// <param name="tokensPerMinute">Null or zero is unlimited.</param>
    /// <param name="tokens">The tokens the request will use (input plus reserved output).</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task WaitAsync(string key, int? requestsPerMinute, int? tokensPerMinute, int tokens, CancellationToken cancellationToken = default)
    {
        if ((requestsPerMinute ?? 0) <= 0 && (tokensPerMinute ?? 0) <= 0) return;
        var window = _windows.GetOrAdd(key, _ => new Window());
        while (true)
        {
            TimeSpan wait;
            lock (window)
            {
                var now = DateTime.UtcNow;
                while (window.Entries.Count > 0 && now - window.Entries.Peek().On >= Period)
                {
                    window.Tokens -= window.Entries.Dequeue().Tokens;
                }

                var requestsFit = (requestsPerMinute ?? 0) <= 0 || window.Entries.Count < requestsPerMinute;
                var tokensFit = (tokensPerMinute ?? 0) <= 0 || window.Tokens + tokens <= tokensPerMinute || window.Entries.Count == 0;
                if (requestsFit && tokensFit)
                {
                    window.Entries.Enqueue((now, tokens));
                    window.Tokens += tokens;
                    return;
                }
                wait = window.Entries.Count > 0 ? Period - (now - window.Entries.Peek().On) : TimeSpan.FromMilliseconds(100);
            }
            await Task.Delay(wait < TimeSpan.FromMilliseconds(50) ? TimeSpan.FromMilliseconds(50) : wait, cancellationToken);
        }
    }

    /// <summary>
    /// The deployment key used to share a window.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    public static string GetKey(LlmEndpoint endpoint) => $"{endpoint.Endpoint}|{endpoint.DeploymentName}";

    private sealed class Window
    {
        public Queue<(DateTime On, int Tokens)> Entries { get; } = new();
        public int Tokens { get; set; }
    }
}
