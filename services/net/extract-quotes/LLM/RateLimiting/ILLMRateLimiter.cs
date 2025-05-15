using System.Threading.RateLimiting;

namespace TNO.Services.ExtractQuotes.LLM.RateLimiting;

/// <summary>
/// Interface for rate limiting LLM API requests
/// </summary>
public interface ILLMRateLimiter
{
    /// <summary>
    /// Acquires a permit from the rate limiter
    /// </summary>
    /// <param name="permitCount">Number of permits to acquire (default: 1)</param>
    /// <returns>A rate limit lease that should be disposed after use</returns>
    Task<RateLimitLease> AcquireAsync(int permitCount = 1);
}
