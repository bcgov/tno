using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    /// <summary>
    /// thread logging information
    /// </summary>
    /// <returns>string</returns>
    private static string GetThreadInfo()
    {
        return $"[Thread {Environment.CurrentManagedThreadId}]";
    }
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
        if (this.Options.LLM == null)
        {
            throw new ArgumentNullException(nameof(this.Options.LLM), "LLM configuration section is missing.");
        }

        if (this.Options.LLM.Primary?.ApiKeys == null || !this.Options.LLM.Primary.ApiKeys.Any())
        {
            throw new ArgumentException("Primary API keys are not configured.");
        }

        if (string.IsNullOrEmpty(this.Options.LLM.Primary?.ModelName))
        {
            throw new ArgumentException("Primary model name is not configured.");
        }

        if (string.IsNullOrEmpty(this.Options.LLM.Primary?.ApiUrl))
        {
            throw new ArgumentException("Primary model API URL is not configured.");
        }

        // Log initialization of primary model
        this.Logger.LogInformation(
            "LLM service initialized - Primary model: {model}, API keys: {keyCount}",
            this.Options.LLM.Primary.ModelName,
            this.Options.LLM.Primary.ApiKeys.Count);

        // Log fallback model if configured
        if (this.Options.LLM.Fallback?.ApiKeys != null && this.Options.LLM.Fallback.ApiKeys.Any() &&
            !string.IsNullOrEmpty(this.Options.LLM.Fallback.ModelName))
        {
            // Validate fallback API URL
            if (string.IsNullOrEmpty(this.Options.LLM.Fallback.ApiUrl))
            {
                throw new ArgumentException("Fallback model API URL is not configured.");
            }

            this.Logger.LogInformation(
                "Fallback LLM configured - Model: {model}, API keys: {keyCount}",
                this.Options.LLM.Fallback.ModelName,
                this.Options.LLM.Fallback.ApiKeys.Count);
        }
        else
        {
            this.Logger.LogWarning("No fallback LLM model configured. Service will not have failover capability.");
        }

    }
    #endregion

    #region Methods

    /// <summary>
    /// Sends the text to the LLM API and returns an AnnotationResponse.
    /// Implements API key rotation and fallback model support.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>Returns the AnnotationResponse with extracted quotes.</returns>
    public async Task<AnnotationResponse?> PerformAnnotation(string text)
    {
        try
        {
            this.Logger.LogInformation("{ThreadInfo} Starting LLM quote extraction - Text length: {length} characters",
                GetThreadInfo(), text.Length);

            // Generate the prompt for quote extraction
            var prompt = _promptGenerator.GenerateQuoteExtractionPrompt(text);

            // 1. Try Primary Model with Key Rotation
            if (this.Options.LLM.Primary?.ApiKeys != null && this.Options.LLM.Primary.ApiKeys.Count > 0)
            {
                string primaryApiKey = _llmClient.GetNextApiKey(this.Options.LLM.Primary.ApiKeys, ref _primaryApiKeyIndex);
                try
                {
                    // calculate the key index
                    int keyIndex = (_primaryApiKeyIndex - 1) % Options.LLM.Primary.ApiKeys.Count;
                    if (keyIndex < 0) keyIndex += Options.LLM.Primary.ApiKeys.Count;
                    this.Logger.LogInformation("{ThreadInfo} Attempting LLM request with primary model '{Model}' using key index {Index}",
                                              GetThreadInfo(), this.Options.LLM.Primary.ModelName, keyIndex);

                    // Rate limiter check
                    using RateLimitLease lease = await _rateLimiter.AcquireAsync();
                    if (!lease.IsAcquired)
                    {
                        this.Logger.LogWarning("Rate limit exceeded for primary LLM API. Request rejected temporarily.");
                        throw new RateLimitRejectedException("LLM rate limit exceeded for primary model.");
                    }

                    // Call the primary model
                    string? responseContent = await _llmClient.CallLLMApiWithPrompt(text, prompt, this.Options.LLM.Primary.ModelName,
                                                                       primaryApiKey);

                    if (responseContent != null)
                    {
                        var annotationResponse = _responseParser.ParseLLMResponse(responseContent, this.Options.LLM.Primary.ModelName);
                        if (annotationResponse != null)
                        {
                            this.Logger.LogInformation("Successfully extracted quotes using primary model '{Model}'.",
                                                      this.Options.LLM.Primary.ModelName);
                            return annotationResponse;
                        }

                        this.Logger.LogWarning("Failed to parse valid quotes from primary model '{Model}' response.",
                                              this.Options.LLM.Primary.ModelName);
                        // Continue to fallback if parsing failed
                    }
                    else
                    {
                        this.Logger.LogWarning("Primary model '{Model}' returned null or empty content.",
                                              this.Options.LLM.Primary.ModelName);
                        // Continue to fallback
                    }
                }
                catch (Exception primaryEx)
                {
                    // Unexpected errors - log but still try fallback
                    this.Logger.LogError(primaryEx, "Unexpected error during primary LLM call for model '{Model}'. Attempting fallback.",
                                        this.Options.LLM.Primary.ModelName);
                }
            }
            else
            {
                this.Logger.LogWarning("Primary LLM configuration is missing or invalid. Skipping primary attempt.");
            }

            // 2. Try Fallback Model if Primary Failed or was Skipped
            if (this.Options.LLM.Fallback?.ApiKeys != null && this.Options.LLM.Fallback.ApiKeys.Count > 0 &&
                !string.IsNullOrEmpty(this.Options.LLM.Fallback.ModelName))
            {
                string fallbackApiKey = _llmClient.GetNextApiKey(this.Options.LLM.Fallback.ApiKeys, ref _fallbackApiKeyIndex);
                try
                {
                    // calculate the key index
                    int keyIndex = (_fallbackApiKeyIndex - 1) % Options.LLM.Fallback.ApiKeys.Count;
                    if (keyIndex < 0) keyIndex += Options.LLM.Fallback.ApiKeys.Count;
                    this.Logger.LogInformation("{ThreadInfo} Attempting LLM request with fallback model '{Model}' using key index {Index}",
                                             GetThreadInfo(), this.Options.LLM.Fallback.ModelName, keyIndex);

                    // Rate limiter check for fallback too
                    using RateLimitLease lease = await _rateLimiter.AcquireAsync();
                    if (!lease.IsAcquired)
                    {
                        this.Logger.LogWarning("Rate limit exceeded for fallback LLM API. Request rejected.");
                        throw new RateLimitRejectedException("LLM rate limit exceeded during fallback.");
                    }

                    // Call the fallback model
                    string? fallbackResponseContent = await _llmClient.CallLLMApiWithPrompt(text, prompt, this.Options.LLM.Fallback.ModelName,
                                                                              fallbackApiKey);

                    if (fallbackResponseContent != null)
                    {
                        var annotationResponse = _responseParser.ParseLLMResponse(fallbackResponseContent, this.Options.LLM.Fallback.ModelName);
                        if (annotationResponse != null)
                        {
                            this.Logger.LogInformation("Successfully extracted quotes using fallback model '{Model}'.",
                                                     this.Options.LLM.Fallback.ModelName);
                            return annotationResponse;
                        }

                        this.Logger.LogWarning("Failed to parse valid quotes from fallback model '{Model}' response.",
                                              this.Options.LLM.Fallback.ModelName);
                        return null; // Both models tried and failed to parse
                    }
                    else
                    {
                        this.Logger.LogError("Fallback model '{Model}' also returned null or empty content.",
                                             this.Options.LLM.Fallback.ModelName);
                        return null; // Both models tried and failed
                    }
                }
                catch (Exception fallbackEx)
                {
                    // Log final failure after fallback attempt
                    this.Logger.LogError(fallbackEx, "Fallback LLM request also failed for model '{Model}'. Giving up for this content.",
                                       this.Options.LLM.Fallback.ModelName);
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


