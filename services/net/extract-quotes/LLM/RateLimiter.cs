using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TNO.Services.ExtractQuotes.LLM
{
    /// <summary>
    /// RateLimiter class, provides rate limiting functionality for API calls.
    /// </summary>
    public class RateLimiter
    {
        private readonly ILogger _logger;
        private readonly int _maxRequestsPerMinute;
        private readonly ConcurrentQueue<DateTime> _requestTimestamps = new ConcurrentQueue<DateTime>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Creates a new instance of a RateLimiter object.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="maxRequestsPerMinute">Maximum number of requests allowed per minute.</param>
        public RateLimiter(ILogger logger, int maxRequestsPerMinute)
        {
            _logger = logger;
            _maxRequestsPerMinute = maxRequestsPerMinute;
            _logger.LogDebug("Rate limiter initialized - Maximum requests per minute: {maxRequests}", maxRequestsPerMinute);
        }

        /// <summary>
        /// Waits until a request can be made without exceeding the rate limits.
        /// </summary>
        /// <returns>A task that completes when the request can be made.</returns>
        public async Task WaitForAvailabilityAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                // Clean up old timestamps (older than 1 minute)
                CleanupOldEntries();

                while (true)
                {
                    int currentRequests = _requestTimestamps.Count;

                    if (currentRequests < _maxRequestsPerMinute)
                    {
                        // We're under the limit, so we can proceed
                        _logger.LogDebug("Rate limit check passed - Current requests: {currentRequests}/{maxRequests}",
                            currentRequests, _maxRequestsPerMinute);
                        break;
                    }

                    _logger.LogInformation(
                        "Rate limit reached - Requests: {currentRequests}/{maxRequests}. Waiting...",
                        currentRequests, _maxRequestsPerMinute);

                    // Release the semaphore while waiting
                    _semaphore.Release();

                    // Wait a bit before checking again
                    await Task.Delay(1000);

                    // Acquire the semaphore again
                    await _semaphore.WaitAsync();

                    // Clean up old timestamps again after waiting
                    CleanupOldEntries();
                }

                // Record this request
                _requestTimestamps.Enqueue(DateTime.UtcNow);
                _logger.LogDebug("New request recorded - Current requests: {currentRequests}/{maxRequests}",
                    _requestTimestamps.Count, _maxRequestsPerMinute);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void CleanupOldEntries()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-1);
            int initialCount = _requestTimestamps.Count;

            // Clean up request timestamps
            while (_requestTimestamps.TryPeek(out var timestamp) && timestamp < cutoff)
            {
                _requestTimestamps.TryDequeue(out _);
            }

            int removedCount = initialCount - _requestTimestamps.Count;
            if (removedCount > 0)
            {
                _logger.LogDebug("Cleaned up {count} expired request timestamps", removedCount);
            }
        }
    }
}
