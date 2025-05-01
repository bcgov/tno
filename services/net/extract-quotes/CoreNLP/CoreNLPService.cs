using TNO.Core.Http;
using Microsoft.Extensions.Options;
using TNO.Services.ExtractQuotes.Config;
using TNO.Services.ExtractQuotes.CoreNLP.models;
using Microsoft.Extensions.Logging;
using TNO.API.Areas.Services.Models.Content;

namespace TNO.Services.NLP.ExtractQuotes;

public interface ICoreNLPService {
    Task<AnnotationResponse?> PerformAnnotation(string text);
    Task<AnnotationResponse?> PerformAnnotationWithExistingQuotes(string text, IEnumerable<QuoteModel> existingQuotes);
}

/// <summary>
/// CoreNLPService class, provides a wrapper service for the Core NLP API.
/// </summary>
public class CoreNLPService: ICoreNLPService
{
    #region Properties
    /// <summary>
    /// get - Number of sequential failures.
    /// </summary>
    public int FailureCount { get; private set; }

    /// <summary>
    /// get - HTTP client.
    /// </summary>
    protected IHttpRequestClient HttpClient { get; }

    /// <summary>
    /// get - Charts options.
    /// </summary>
    protected ExtractQuotesOptions Options { get; }

    /// <summary>
    /// get - Logger for this service.
    /// </summary>
    public ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportEngine object, initializes with specified parameters.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="extractQuotesOptions"></param>
    public CoreNLPService(
        IHttpRequestClient httpClient,
        IOptions<ExtractQuotesOptions> extractQuotesOptions,
        ILogger<CoreNLPService> logger)
    {
        this.HttpClient = httpClient;
        this.Options = extractQuotesOptions.Value;
        this.Logger = logger;
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Depending on configuration the function will either throw the exception if it occurs, or it will return the specified 'defaultResponse'.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callbackDelegate"></param>
    /// <param name="ignoreError"></param>
    /// <param name="defaultResponse"></param>
    /// <returns></returns>
    public async Task<T> HandleRequestFailure<T>(Func<Task<T>> callbackDelegate, bool ignoreError, T defaultResponse)
    {
        try
        {
            return await callbackDelegate();
        }
        catch (Exception ex)
        {
            // If configured to reuse existing ingests it will ignore the error and continue running.
            if (!ignoreError)
                throw;

            this.Logger.LogError(ex, "Ignoring error and reusing existing ingests");
            return defaultResponse;
        }
    }

    /// <summary>
    /// Self referencing retry method that will log the error and retry the configured number of attempts.
    /// Delays the configured number of milliseconds before retrying.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    protected async Task<T> RetryRequestAsync<T>(Func<Task<T>> callbackDelegate)
    {
        try
        {
            var response = await callbackDelegate();
            this.FailureCount = 0;
            return response;
        }
        catch (Exception ex)
        {
            if (this.Options.RetryLimit <= ++this.FailureCount)
            {
                this.FailureCount = 0;
                throw;
            }

            // Wait before retrying.
            this.Logger.LogError(ex, "Retry attempt {count}.{newline}Error:{body}", this.FailureCount, Environment.NewLine, ex.Data["Body"]);
            await Task.Delay(this.Options.RetryDelayMS);
            return await RetryRequestAsync<T>(callbackDelegate);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Sends the text to the CoreNLP endpoint and returns an AnnotationResponse.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    public async Task<AnnotationResponse?> PerformAnnotation(
        string text)
    {
        // Send request to CoreNLP API to annotate text in request
        var body = new StringContent(text);
        return await RetryRequestAsync(async () => await this.HttpClient.PostAsync<AnnotationResponse>(this.Options.CoreNLPApiUrl, body));
    }

    /// <summary>
    /// Performs annotation with existing quotes to exclude
    /// </summary>
    /// <param name="text">The text to analyze</param>
    /// <param name="existingQuotes">Existing quotes to exclude from results</param>
    /// <returns>Annotation response with quotes</returns>
    public async Task<AnnotationResponse?> PerformAnnotationWithExistingQuotes(string text, IEnumerable<QuoteModel> existingQuotes)
    {
        // For the CoreNLP implementation, we just call the regular method and filter the results later
        // This is because CoreNLP doesn't support excluding quotes in the API
        this.Logger.LogInformation("CoreNLP implementation ignores existing quotes parameter");
        return await PerformAnnotation(text);
    }
    #endregion
}