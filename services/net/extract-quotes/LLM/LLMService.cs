using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Http;
using TNO.Services.ExtractQuotes.Config;
using TNO.Services.ExtractQuotes.CoreNLP.models;
using TNO.Services.NLP.ExtractQuotes;
namespace TNO.Services.ExtractQuotes.LLM;

/// <summary>
/// LLMService class, provides a way to extract quotes using a third-party LLM API compatible with OpenAI format.
/// </summary>
public class LLMService : ICoreNLPService
{
    #region Variables
    private readonly IHttpRequestClient HttpClient;
    private readonly ILogger<LLMService> Logger;
    private readonly ExtractQuotesOptions Options;
    private readonly TokenBucketRateLimiter _rateLimiter;
    private int _failureCount = 0;
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
    /// <param name="httpClient"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public LLMService(
        IHttpRequestClient httpClient,
        IOptions<ExtractQuotesOptions> options,
        ILogger<LLMService> logger)
    {
        this.HttpClient = httpClient;
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

        // Initialize the rate limiter with configured limits
        _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = this.Options.LLM.MaxRequestsPerMinute,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0, // No queuing, fail fast if rate limit is exceeded
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = this.Options.LLM.MaxRequestsPerMinute,
            AutoReplenishment = true
        });

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
    /// Retry the specified request if it fails.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callbackDelegate"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<T?> RetryRequestAsync<T>(Func<Task<T?>> callbackDelegate)
    {
        try
        {
            return await callbackDelegate();
        }
        catch (Exception ex)
        {
            // Use Interlocked.Increment for thread-safe increment and wrap-around.
            int currentFailureCount = Interlocked.Increment(ref _failureCount);
            if (this.Options.RetryLimit <= currentFailureCount)
            {
                Interlocked.Exchange(ref _failureCount, 0); // Reset the counter
                throw;
            }

            // Wait before retrying.
            this.Logger.LogError(ex, "LLM API retry attempt {count}.{newline}Error:{body}",
                currentFailureCount, Environment.NewLine, ex.Data["Body"]);
            await Task.Delay(this.Options.RetryDelayMS);
            return await RetryRequestAsync<T>(callbackDelegate);
        }
    }

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

            // Create the prompt for the LLM (still based on the original method)
            var prompt = $@"Extract all direct quotes from the following text. For each quote, identify the speaker.
If the speaker is not explicitly mentioned, use 'Unknown' as the speaker name.

Text:
{text}

Return the result in the following JSON format:
{{
  ""quotes"": [
    {{
      ""id"": 1,
      ""text"": ""The exact quote text including quotation marks"",
      ""canonicalSpeaker"": ""The name of the speaker"",
      ""beginSentence"": 0
    }},
    ...
  ]
}}

IMPORTANT FORMATTING INSTRUCTIONS:
1. Only include quotes that are explicitly marked with quotation marks in the text.
2. When a quote contains single quotes (') or other special characters, properly escape them in the JSON.
3. Make sure the JSON is valid and can be parsed by a standard JSON parser.
4. Do not include trailing commas in JSON arrays or objects.
5. Ensure all quotes and property names use double quotes in the JSON output.
6. If a quote contains nested quotes, properly escape the nested quotes with a backslash.
7. The 'text' field should contain the exact quote as it appears in the text, preserving all punctuation.";

            // 1. Try Primary Model with Key Rotation
            if (this.Options.LLM.Primary?.ApiKeys != null && this.Options.LLM.Primary.ApiKeys.Count > 0)
            {
                string primaryApiKey = GetNextApiKey(this.Options.LLM.Primary.ApiKeys, ref _primaryApiKeyIndex);
                try
                {
                    // calculate the key index
                    int keyIndex = (_primaryApiKeyIndex - 1) % Options.LLM.Primary.ApiKeys.Count;
                    if (keyIndex < 0) keyIndex += Options.LLM.Primary.ApiKeys.Count;
                    this.Logger.LogInformation("{ThreadInfo} Attempting LLM request with primary model '{Model}' using key index {Index}",
                                              GetThreadInfo(), this.Options.LLM.Primary.ModelName, keyIndex);

                    // Rate limiter check
                    using RateLimitLease lease = await _rateLimiter.AcquireAsync(1);
                    if (!lease.IsAcquired)
                    {
                        this.Logger.LogWarning("Rate limit exceeded for primary LLM API. Request rejected temporarily.");
                        throw new RateLimitRejectedException("LLM rate limit exceeded for primary model.");
                    }

                    // Call the primary model
                    string? responseContent = await CallLLMApiWithPrompt(text, prompt, this.Options.LLM.Primary.ModelName,
                                                                       primaryApiKey);

                    if (responseContent != null)
                    {
                        var annotationResponse = ParseLLMResponse(responseContent, this.Options.LLM.Primary.ModelName);
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
                catch (Exception primaryEx) when (primaryEx is HttpRequestException
                                              || primaryEx is TaskCanceledException
                                              || primaryEx is OperationCanceledException
                                              || primaryEx is TimeoutException
                                              || primaryEx is RateLimitRejectedException)
                {
                    // Transient errors - try fallback
                    this.Logger.LogWarning(primaryEx, "Primary LLM request failed for model '{Model}'. Attempting fallback.",
                                         this.Options.LLM.Primary.ModelName);
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
                string fallbackApiKey = GetNextApiKey(this.Options.LLM.Fallback.ApiKeys, ref _fallbackApiKeyIndex);
                try
                {
                    // calculate the key index
                    int keyIndex = (_fallbackApiKeyIndex - 1) % Options.LLM.Fallback.ApiKeys.Count;
                    if (keyIndex < 0) keyIndex += Options.LLM.Fallback.ApiKeys.Count;
                    this.Logger.LogInformation("{ThreadInfo} Attempting LLM request with fallback model '{Model}' using key index {Index}",
                                             GetThreadInfo(), this.Options.LLM.Fallback.ModelName, keyIndex);

                    // Rate limiter check for fallback too
                    using RateLimitLease lease = await _rateLimiter.AcquireAsync(1);
                    if (!lease.IsAcquired)
                    {
                        this.Logger.LogWarning("Rate limit exceeded for fallback LLM API. Request rejected.");
                        throw new RateLimitRejectedException("LLM rate limit exceeded during fallback.");
                    }

                    // Call the fallback model
                    string? fallbackResponseContent = await CallLLMApiWithPrompt(text, prompt, this.Options.LLM.Fallback.ModelName,
                                                                              fallbackApiKey);

                    if (fallbackResponseContent != null)
                    {
                        var annotationResponse = ParseLLMResponse(fallbackResponseContent, this.Options.LLM.Fallback.ModelName);
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

    /// <summary>
    /// Helper method that handles creating the full request payload with prompt and calling the LLM API.
    /// </summary>
    private async Task<string?> CallLLMApiWithPrompt(string text, string prompt, string modelName, string apiKey)
    {
        // Create the request body for the LLM API
        var requestBody = new
        {
            model = modelName,
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that extracts quotes from text." },
                new { role = "user", content = prompt }
            },
            temperature = 0.0,
            max_tokens = 4000
        };

        // Convert the request body to JSON
        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Determine which API URL to use based on the model name
        string apiUrl;

        // Check if we're using the primary model
        if (modelName == this.Options.LLM.Primary.ModelName)
        {
            apiUrl = this.Options.LLM.Primary.ApiUrl;
        }
        // Check if we're using the fallback model
        else if (modelName == this.Options.LLM.Fallback.ModelName)
        {
            apiUrl = this.Options.LLM.Fallback.ApiUrl;
        }
        // If neither, throw an exception
        else
        {
            throw new ArgumentException($"No API URL configured for model {modelName}");
        }

        // Create headers with the API key - use HttpRequestMessage.Headers instead of Dictionary
        var headers = new HttpRequestMessage().Headers;
        headers.Add("Authorization", $"Bearer {apiKey}");

        try
        {
            // Send the request to the LLM API - using SendAsync which is the method in the original code
            if (string.IsNullOrEmpty(apiUrl))
            {
                throw new ArgumentException("Argument 'url' must be a valid URL.");
            }

            this.Logger.LogInformation("{ThreadInfo} Sending request to LLM API: {url} with model: {model}",
                GetThreadInfo(), apiUrl, modelName);

            // Fix parameter order and type, call using original method
            LLMResponse? response = await RetryRequestAsync(async () =>
                await this.HttpClient.SendAsync<LLMResponse>(apiUrl, HttpMethod.Post, headers, content));

            if (response == null)
            {
                this.Logger.LogWarning("LLM API call to model '{Model}' returned null response.", modelName);
                return null;
            }

            // Extract the content from the response
            var responseContent = response.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(responseContent))
            {
                this.Logger.LogWarning("LLM response from model '{Model}' did not contain message content.", modelName);
                return null;
            }

            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP-level errors (network, DNS, non-success status codes)
            this.Logger.LogError(ex, "HTTP error during LLM API call for model '{Model}'. Status Code: {StatusCode}", modelName, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Unexpected error during LLM API call for model '{Model}'.", modelName);
            throw;
        }
    }

    /// <summary>
    /// Gets the next API key from the list in a thread-safe, round-robin manner.
    /// </summary>
    /// <param name="keys">List of API keys.</param>
    /// <param name="indexField">Reference to the index field for this list.</param>
    /// <returns>The next API key.</returns>
    private static string GetNextApiKey(List<string> keys, ref int indexField)
    {
        if (keys == null || keys.Count == 0)
        {
            throw new InvalidOperationException("No API keys available for rotation.");
        }
        // Interlocked.Increment safely increments the indexField
        int currentIndex = Interlocked.Increment(ref indexField);
        return keys[(currentIndex - 1) % keys.Count];
    }

    /// <summary>
    /// Parses the raw string content from the LLM response into an AnnotationResponse.
    /// </summary>
    /// <param name="llmResponseContent">The string content from the LLM response.</param>
    /// <param name="modelName">The name of the model that generated the response (for logging).</param>
    /// <returns>AnnotationResponse if parsing is successful, otherwise null.</returns>
    private AnnotationResponse? ParseLLMResponse(string llmResponseContent, string modelName)
    {
        // Existing JSON extraction logic - keep or adapt
        int jsonStartIndex = llmResponseContent.IndexOf('{');
        int jsonEndIndex = llmResponseContent.LastIndexOf('}');

        if (jsonStartIndex >= 0 && jsonEndIndex > jsonStartIndex)
        {
            var jsonString = llmResponseContent.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);

            try
            {
                // Assumes QuoteResponse structure defined below matches the expected JSON
                var quoteResponse = JsonSerializer.Deserialize<QuoteResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (quoteResponse != null && quoteResponse.quotes != null && quoteResponse.quotes.Count > 0)
                {
                    this.Logger.LogInformation("Successfully parsed {count} quotes from LLM response (Model: {Model})",
                        quoteResponse.quotes.Count, modelName);

                    // --- Conversion logic from QuoteResponse to AnnotationResponse ---
                    var quotes = quoteResponse.quotes.Select((q, i) => new Quote
                    {
                        id = i + 1, // Generate ID based on order
                        text = q.Text?.Trim() ?? "", // Ensure text is not null and trim whitespace
                        canonicalSpeaker = q.CanonicalSpeaker?.Trim() ?? "Unknown", // Ensure speaker is not null and trim
                        beginSentence = q.BeginSentence // Use provided sentence index
                    }).ToList();

                    // Create sentences structure based on max beginSentence index
                    int maxSentenceIndex = quotes.Count > 0 ? quotes.Max(q => q.beginSentence) : -1;
                    var sentences = Enumerable.Range(0, maxSentenceIndex + 1)
                                        .Select(i => new Sentence { index = i, entityMentions = new List<EntityMention>() })
                                        .ToList();

                    // Add entity mentions for speakers to the correct sentence
                    foreach (var quote in quotes)
                    {
                        if (quote.beginSentence >= 0 && quote.beginSentence < sentences.Count && !string.IsNullOrWhiteSpace(quote.canonicalSpeaker))
                        {
                            // Avoid adding duplicate speakers to the same sentence if multiple quotes start there
                            if (!sentences[quote.beginSentence].entityMentions.Any(em => em.text == quote.canonicalSpeaker))
                            {
                                sentences[quote.beginSentence].entityMentions.Add(new EntityMention
                                {
                                    text = quote.canonicalSpeaker,
                                    ner = "PERSON" // Assuming speaker is always PERSON
                                });
                            }
                        }
                    }
                    this.Logger.LogDebug("Created {count} sentences structure based on quote indices.", sentences.Count);
                    // --- End Conversion Logic ---

                    var annotationResponse = new AnnotationResponse
                    {
                        Quotes = quotes,
                        Sentences = sentences
                    };

                    // Log each extracted quote for debugging
                    foreach (var quote in annotationResponse.Quotes)
                    {
                        this.Logger.LogDebug("Extracted quote: Speaker='{speaker}', Text='{text}'",
                            quote.canonicalSpeaker, quote.text);
                    }

                    return annotationResponse;
                }
                else
                {
                    this.Logger.LogWarning("Parsed JSON from model '{Model}' does not contain valid quotes or is empty. JSON: {json}", modelName, jsonString);
                    return null;
                }
            }
            catch (JsonException ex)
            {
                this.Logger.LogError(ex, "Failed to parse LLM response JSON from model '{Model}'. JSON: {json}", modelName, jsonString);
                return null;
            }
        }
        else
        {
            this.Logger.LogWarning("Unable to extract JSON object from LLM response content from model '{Model}'. Content: {content}", modelName, llmResponseContent);
            return null;
        }
    }

    #endregion
}

/// <summary>
/// Response from the LLM API
/// </summary>
public class LLMResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public List<Choice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// Quote response from the LLM
/// </summary>
public class QuoteResponse
{
    public List<QuoteItem> quotes { get; set; } = new List<QuoteItem>();
}

public class QuoteItem
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public string? CanonicalSpeaker { get; set; }
    public int BeginSentence { get; set; }
}

/// <summary>
/// Exception thrown when a rate limit is exceeded and the request is rejected.
/// </summary>
public class RateLimitRejectedException : Exception
{
    public RateLimitRejectedException(string message) : base(message) { }
    public RateLimitRejectedException(string message, Exception innerException) : base(message, innerException) { }
}
