using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private readonly RateLimiter RateLimiter;
    private int FailureCount = 0;
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

        // Initialize the rate limiter with configured limits
        this.RateLimiter = new RateLimiter(
            logger,
            this.Options.LLMMaxRequestsPerMinute);

        this.Logger.LogInformation(
            "LLM service initialized - API URL: {apiUrl}, Model: {model}, Rate limit: {requestLimit} requests/minute",
            this.Options.LLMApiUrl,
            this.Options.LLMModelName,
            this.Options.LLMMaxRequestsPerMinute);
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
            if (this.Options.RetryLimit <= ++this.FailureCount)
            {
                this.FailureCount = 0;
                throw;
            }

            // Wait before retrying.
            this.Logger.LogError(ex, "LLM API retry attempt {count}.{newline}Error:{body}", this.FailureCount, Environment.NewLine, ex.Data["Body"]);
            await Task.Delay(this.Options.RetryDelayMS);
            return await RetryRequestAsync<T>(callbackDelegate);
        }
    }

    /// <summary>
    /// Sends the text to the LLM API and returns an AnnotationResponse.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>Returns the AnnotationResponse with extracted quotes.</returns>
    public async Task<AnnotationResponse?> PerformAnnotation(string text)
    {
        try
        {
            this.Logger.LogInformation("Starting LLM quote extraction - Text length: {length} characters", text.Length);
            this.Logger.LogDebug("Using LLM API URL: {url}", this.Options.LLMApiUrl);
            this.Logger.LogDebug("Using LLM model: {model}", this.Options.LLMModelName);

            // Wait for rate limiter to allow this request
            await this.RateLimiter.WaitForAvailabilityAsync();

            // Create the prompt for the LLM
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

Only include quotes that are explicitly marked with quotation marks in the text.";

            // Create the request body for the LLM API
            var requestBody = new
            {
                model = this.Options.LLMModelName,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that extracts quotes from text." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.0,
                max_tokens = 4000
            };

            this.Logger.LogDebug("LLM prompt: {prompt}", prompt);

            // Convert the request body to JSON
            var jsonContent = JsonSerializer.Serialize(requestBody);
            this.Logger.LogDebug("Request JSON: {json}", jsonContent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Create headers with the API key
            var headers = new HttpRequestMessage().Headers;
            headers.Add("Authorization", $"Bearer {this.Options.LLMApiKey}");

            // Send the request to the LLM API
            this.Logger.LogInformation("Sending request to LLM API: {url}", this.Options.LLMApiUrl);
            var response = await RetryRequestAsync(async () =>
                await this.HttpClient.SendAsync<LLMResponse>(this.Options.LLMApiUrl, HttpMethod.Post, headers, content));

            if (response == null)
            {
                this.Logger.LogWarning("LLM API returned empty response");
                return null;
            }

            this.Logger.LogInformation("Received LLM API response - Status: {status}", response.Status);

            // Extract the content from the response
            var responseContent = response.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(responseContent))
            {
                this.Logger.LogWarning("No content in LLM response");
                return null;
            }

            this.Logger.LogDebug("LLM response content: {content}", responseContent);

            // Try to extract JSON from the response
            int jsonStartIndex = responseContent.IndexOf('{');
            int jsonEndIndex = responseContent.LastIndexOf('}');

            if (jsonStartIndex >= 0 && jsonEndIndex > jsonStartIndex)
            {
                var jsonString = responseContent.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                this.Logger.LogDebug("JSON extracted from response: {json}", jsonString);

                try
                {
                    var quoteResponse = JsonSerializer.Deserialize<QuoteResponse>(jsonString);

                    if (quoteResponse != null && quoteResponse.quotes != null)
                    {
                        this.Logger.LogInformation("Successfully parsed {count} quotes from LLM response",
                            quoteResponse.quotes.Count);

                        // Convert to AnnotationResponse format
                        var quotes = quoteResponse.quotes.Select((q, i) => new Quote
                        {
                            id = i + 1,
                            text = q.text,
                            canonicalSpeaker = q.canonicalSpeaker,
                            beginSentence = q.beginSentence
                        }).ToList();

                        // Create sentences for each quote
                        var sentences = new List<Sentence>();
                        for (int i = 0; i < quotes.Count; i++)
                        {
                            var quote = quotes[i];

                            // Create a sentence for each quote if it doesn't exist
                            while (sentences.Count <= quote.beginSentence)
                            {
                                sentences.Add(new Sentence
                                {
                                    index = sentences.Count,
                                    entityMentions = new List<EntityMention>()
                                });

                                // Add an entity mention for the speaker
                                sentences.Last().entityMentions.Add(new EntityMention
                                {
                                    text = quote.canonicalSpeaker,
                                    ner = "PERSON"
                                });
                            }
                        }

                        this.Logger.LogDebug("Created {count} sentences with entity mentions", sentences.Count);

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
                        this.Logger.LogWarning("Parsed response does not contain quotes");
                    }
                }
                catch (JsonException ex)
                {
                    this.Logger.LogError(ex, "Failed to parse LLM response JSON: {json}", jsonString);
                }
            }
            else
            {
                this.Logger.LogWarning("Unable to extract JSON from LLM response");
            }

            return null;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error occurred during LLM quote extraction");
            throw;
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
    public int id { get; set; }
    public string text { get; set; } = "";
    public string canonicalSpeaker { get; set; } = "";
    public int beginSentence { get; set; }
}
