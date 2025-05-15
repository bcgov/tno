namespace TNO.Services.ExtractQuotes.LLM.Exceptions;

/// <summary>
/// Exception thrown when a rate limit is exceeded and the request is rejected.
/// </summary>
public class RateLimitRejectedException : Exception
{
    public RateLimitRejectedException(string message) : base(message) { }
    public RateLimitRejectedException(string message, Exception innerException) : base(message, innerException) { }
}
