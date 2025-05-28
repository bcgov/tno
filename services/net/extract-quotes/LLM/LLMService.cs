using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Services.ExtractQuotes.Config;
using TNO.Services.ExtractQuotes.CoreNLP.models;
using TNO.Services.ExtractQuotes.LLM.Clients;
using TNO.Services.ExtractQuotes.LLM.Exceptions;
using TNO.Services.ExtractQuotes.LLM.Parsers;
using TNO.Services.ExtractQuotes.LLM.Prompts;
using TNO.Services.ExtractQuotes.LLM.RateLimiting;
using TNO.Services.NLP.ExtractQuotes;
namespace TNO.Services.ExtractQuotes.LLM;

/// <summary>
/// LLMService class, provides a way to extract quotes using a third-party LLM API compatible with OpenAI format.
/// </summary>
public class LLMService : ICoreNLPService
{
    #region Variables
    private readonly ILLMClient _llmClient;
    private readonly ILLMResponseParser _responseParser;
    private readonly IPromptGenerator _promptGenerator;
    private readonly ILLMRateLimiter _rateLimiter;
    private readonly ILogger<LLMService> Logger;
    private readonly ExtractQuotesOptions Options;
    private int _primaryApiKeyIndex = 0;
    private int _fallbackApiKeyIndex = 0;

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LLMService object, initializes with specified parameters.
    /// </summary>
    /// <param name="llmClient">Client for making LLM API calls</param>
    /// <param name="responseParser">Parser for LLM responses</param>
    /// <param name="promptGenerator">Generator for LLM prompts</param>
    /// <param name="rateLimiter">Rate limiter for LLM API calls</param>
    /// <param name="options">Service configuration options</param>
    /// <param name="logger">Logger for this service</param>
    public LLMService(
        ILLMClient llmClient,
        ILLMResponseParser responseParser,
        IPromptGenerator promptGenerator,
        ILLMRateLimiter rateLimiter,
        IOptions<ExtractQuotesOptions> options,
        ILogger<LLMService> logger)
    {
        _llmClient = llmClient;
        _responseParser = responseParser;
        _promptGenerator = promptGenerator;
        _rateLimiter = rateLimiter;
        this.Options = options.Value;
        this.Logger = logger;

        // Validate the LLM configuration
        if (string.IsNullOrWhiteSpace(this.Options.PrimaryApiKeys))
        {
            throw new ArgumentException("Primary API keys are not configured.");
        }

        if (string.IsNullOrEmpty(this.Options.PrimaryModelName))
        {
            throw new ArgumentException("Primary model name is not configured.");
        }

        if (string.IsNullOrEmpty(this.Options.PrimaryApiUrl))
        {
            throw new ArgumentException("Primary model API URL is not configured.");
        }

        // Get the list of API keys for logging
        var primaryKeysList = this.Options.GetPrimaryApiKeysList();

        // Log initialization of primary model
        this.Logger.LogInformation(
            "LLM service initialized - Primary model: {model}, API keys: {keyCount}",
            this.Options.PrimaryModelName,
            primaryKeysList.Count);

        // Log fallback model if configured
        if (!string.IsNullOrWhiteSpace(this.Options.FallbackApiKeys) &&
            !string.IsNullOrEmpty(this.Options.FallbackModelName))
        {
            // Validate fallback API URL
            if (string.IsNullOrEmpty(this.Options.FallbackApiUrl))
            {
                throw new ArgumentException("Fallback model API URL is not configured.");
            }

            var fallbackKeysList = this.Options.GetFallbackApiKeysList();

            this.Logger.LogInformation(
                "Fallback LLM configured - Model: {model}, API keys: {keyCount}",
                this.Options.FallbackModelName,
                fallbackKeysList.Count);
        }
        else
        {
            this.Logger.LogWarning("No fallback LLM model configured. Service will not have failover capability.");
        }

    }
    #endregion

    #region Methods

    /// <summary>
    /// thread logging information
    /// </summary>
    /// <returns>string</returns>
    private static string GetThreadInfo()
    {
        return $"[Thread {Environment.CurrentManagedThreadId}]";
    }


    /// <summary>
    /// Sends the text to the LLM API and returns an AnnotationResponse.
    /// Implements API key rotation and fallback model support.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>Returns the AnnotationResponse with extracted quotes.</returns>
    public async Task<AnnotationResponse?> PerformAnnotation(string text)
    {
        // Call the implementation with no existing quotes
        return await PerformAnnotationWithExistingQuotes(text, Enumerable.Empty<QuoteModel>());
    }

    /// <summary>
    /// Performs annotation with existing quotes to exclude
    /// </summary>
    /// <param name="text">The text to analyze</param>
    /// <param name="existingQuotes">Existing quotes to exclude from results</param>
    /// <returns>Annotation response with quotes</returns>
    public async Task<AnnotationResponse?> PerformAnnotationWithExistingQuotes(string text, IEnumerable<QuoteModel> existingQuotes)
    {
        try
        {
            this.Logger.LogInformation("{ThreadInfo} Starting LLM quote extraction - Text length: {length} characters, Existing quotes: {count}",
                GetThreadInfo(), text.Length, existingQuotes.Count());

            // Generate the prompt for quote extraction, including existing quotes
            var prompt = _promptGenerator.GenerateQuoteExtractionPrompt(text, existingQuotes);

            Logger.LogDebug("{ThreadInfo} Using prompt template: {prompt}", GetThreadInfo(), prompt);

            // 1. Try Primary Model with Key Rotation
            if (!string.IsNullOrWhiteSpace(this.Options.PrimaryApiKeys))
            {
                string primaryApiKey = _llmClient.GetNextApiKeyFromString(this.Options.PrimaryApiKeys, ref _primaryApiKeyIndex);
                try
                {
                    // Get the list of API keys for index calculation
                    var primaryKeysList = this.Options.GetPrimaryApiKeysList();

                    // calculate the key index
                    int keyIndex = (_primaryApiKeyIndex - 1) % primaryKeysList.Count;
                    if (keyIndex < 0) keyIndex += primaryKeysList.Count;
                    this.Logger.LogInformation("{ThreadInfo} Attempting LLM request with primary model '{Model}' using key index {Index}",
                                              GetThreadInfo(), this.Options.PrimaryModelName, keyIndex);

                    // Call the primary model
                    string? responseContent = await _llmClient.CallLLMApiWithPrompt(text, prompt, this.Options.PrimaryModelName,
                                                                       primaryApiKey);

                    if (responseContent != null)
                    {
                        var annotationResponse = _responseParser.ParseLLMResponse(responseContent, this.Options.PrimaryModelName);
                        if (annotationResponse != null)
                        {
                            int quoteCount = annotationResponse.Quotes.Count;
                            this.Logger.LogInformation("Primary model '{Model}' processing complete. Found {count} quotes.",
                                                    this.Options.PrimaryModelName, quoteCount);
                            return annotationResponse;
                        }

                        this.Logger.LogWarning("Failed to parse response from primary model '{Model}'.",
                                              this.Options.PrimaryModelName);
                        // Continue to fallback if parsing failed
                    }
                    else
                    {
                        this.Logger.LogWarning("Primary model '{Model}' returned null or empty content.",
                                              this.Options.PrimaryModelName);
                        // Continue to fallback
                    }
                }
                catch (Exception primaryEx)
                {
                    // Unexpected errors - log but still try fallback
                    this.Logger.LogError(primaryEx, "Unexpected error during primary LLM call for model '{Model}'. Attempting fallback.",
                                        this.Options.PrimaryModelName);
                }
            }
            else
            {
                this.Logger.LogWarning("Primary LLM configuration is missing or invalid. Skipping primary attempt.");
            }

            // 2. Try Fallback Model if Primary Failed or was Skipped
            if (!string.IsNullOrWhiteSpace(this.Options.FallbackApiKeys) &&
                !string.IsNullOrEmpty(this.Options.FallbackModelName))
            {
                string fallbackApiKey = _llmClient.GetNextApiKeyFromString(this.Options.FallbackApiKeys, ref _fallbackApiKeyIndex);
                try
                {
                    // Get the list of API keys for index calculation
                    var fallbackKeysList = this.Options.GetFallbackApiKeysList();

                    // calculate the key index
                    int keyIndex = (_fallbackApiKeyIndex - 1) % fallbackKeysList.Count;
                    if (keyIndex < 0) keyIndex += fallbackKeysList.Count;
                    this.Logger.LogInformation("{ThreadInfo} Attempting LLM request with fallback model '{Model}' using key index {Index}",
                                             GetThreadInfo(), this.Options.FallbackModelName, keyIndex);

                    // Call the fallback model
                    string? fallbackResponseContent = await _llmClient.CallLLMApiWithPrompt(text, prompt, this.Options.FallbackModelName,
                                                                              fallbackApiKey);

                    if (fallbackResponseContent != null)
                    {
                        var annotationResponse = _responseParser.ParseLLMResponse(fallbackResponseContent, this.Options.FallbackModelName);
                        if (annotationResponse != null)
                        {
                            // sometimes contents doesn't have any quotes, so we still want to return an empty response
                            int quoteCount = annotationResponse.Quotes.Count;
                            this.Logger.LogInformation("Fallback model '{Model}' processing complete. Found {count} quotes.",
                                                    this.Options.FallbackModelName, quoteCount);
                            return annotationResponse;
                        }

                        this.Logger.LogWarning("Failed to parse response from fallback model '{Model}'.",
                                              this.Options.FallbackModelName);
                        return null; // Both models tried and failed to parse
                    }
                    else
                    {
                        this.Logger.LogError("Fallback model '{Model}' also returned null or empty content.",
                                             this.Options.FallbackModelName);
                        return null; // Both models tried and failed
                    }
                }
                catch (Exception fallbackEx)
                {
                    // Log final failure after fallback attempt
                    this.Logger.LogError(fallbackEx, "Fallback LLM request also failed for model '{Model}'. Giving up for this content.",
                                       this.Options.FallbackModelName);
                    return null; // Both models tried and failed
                }
            }
            else
            {
                this.Logger.LogWarning("No fallback LLM configured after primary model failure. No quotes extracted.");
                return null; // No fallback configured
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error occurred during LLM quote extraction");
            throw;
        }
    }



    #endregion
}


