using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.ExtractQuotes.Config;

namespace TNO.Services.ExtractQuotes.LLM.RateLimiting;

/// <summary>
/// Service for rate limiting LLM API requests
/// </summary>
public class LLMRateLimiter : ILLMRateLimiter, IDisposable
{
    private readonly TokenBucketRateLimiter _rateLimiter;
    private readonly ILogger<LLMRateLimiter> _logger;
    private bool _disposed = false;

    /// <summary>
    /// Creates a new instance of LLMRateLimiter
    /// </summary>
    /// <param name="options">Service configuration options</param>
    /// <param name="logger">Logger for this service</param>
    public LLMRateLimiter(
        IOptions<ExtractQuotesOptions> options,
        ILogger<LLMRateLimiter> logger)
    {
        _logger = logger;
        
        var maxRequestsPerMinute = options.Value.LLM.MaxRequestsPerMinute;
        _logger.LogInformation("Initializing rate limiter with {count} requests per minute", maxRequestsPerMinute);

        // Initialize the rate limiter with configured limits
        _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = maxRequestsPerMinute,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0, // No queuing, fail fast if rate limit is exceeded
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = maxRequestsPerMinute,
            AutoReplenishment = true
        });
    }

    /// <summary>
    /// Acquires a permit from the rate limiter
    /// </summary>
    /// <param name="permitCount">Number of permits to acquire (default: 1)</param>
    /// <returns>A rate limit lease that should be disposed after use</returns>
    public async Task<RateLimitLease> AcquireAsync(int permitCount = 1)
    {
        _logger.LogDebug("Attempting to acquire {count} permit(s) from rate limiter", permitCount);
        return await _rateLimiter.AcquireAsync(permitCount);
    }

    /// <summary>
    /// Disposes the rate limiter
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the rate limiter
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _rateLimiter.Dispose();
            }

            _disposed = true;
        }
    }
}
