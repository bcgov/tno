using System.Net;

namespace TNO.AI;

/// <summary>
/// LlmConfigurationException class, the LLM is not configured well enough to send a request. The
/// fault is the configuration's, not the work's, so the work is retried once it is fixed.
/// </summary>
public class LlmConfigurationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new instance of a LlmConfigurationException.
    /// </summary>
    /// <param name="message"></param>
    public LlmConfigurationException(string message) : base(message) { }

    /// <summary>
    /// Whether the exception is a configuration problem: the LLM is missing a setting, or the
    /// provider rejected its key (401), its access (403), or its endpoint or deployment (404).
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsConfigurationError(Exception ex) =>
        ex is LlmConfigurationException
        || ex is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound };
}
