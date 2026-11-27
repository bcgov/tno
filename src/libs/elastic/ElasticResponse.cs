namespace TNO.Elastic;

/// <summary>
/// ElasticResponse class, the unparsed result of a request made with ITNOElasticClient.RequestAsync.
/// An unsuccessful status is returned rather than thrown, so callers can act on it (a HEAD that
/// answers 404, a task that has gone).
/// </summary>
public sealed class ElasticResponse
{
    #region Properties
    /// <summary>
    /// get - The HTTP status code, or null when no response was received.
    /// </summary>
    public int? HttpStatusCode { get; }

    /// <summary>
    /// get - The response body ("" when there was none).
    /// </summary>
    public string Body { get; }

    /// <summary>
    /// get - The exception raised while making the request, when there was one.
    /// </summary>
    public Exception? OriginalException { get; }

    /// <summary>
    /// get - Whether Elasticsearch answered with a 2xx status.
    /// </summary>
    public bool Success => this.HttpStatusCode is >= 200 and < 300;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ElasticResponse object, initializes with specified parameters.
    /// </summary>
    /// <param name="httpStatusCode"></param>
    /// <param name="body"></param>
    /// <param name="originalException"></param>
    public ElasticResponse(int? httpStatusCode, string? body, Exception? originalException = null)
    {
        this.HttpStatusCode = httpStatusCode;
        this.Body = body ?? "";
        this.OriginalException = originalException;
    }
    #endregion
}
