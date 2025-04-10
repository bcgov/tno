using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
            this.Logger.LogError(ex, "Retry attempt {count}.{newline}Error:{body}", this.FailureCount, Environment.NewLine, ex.Data["Body"]);
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
            this.Logger.LogInformation("Starting LLM quote extraction with text length: {length}", text.Length);
            this.Logger.LogDebug("Using LLM API URL: {url}", this.Options.LLMApiUrl);
            this.Logger.LogDebug("Using LLM model: {model}", this.Options.LLMModelName);
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

            this.Logger.LogDebug("Prompt for LLM: {prompt}", prompt);

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
                this.Logger.LogWarning("LLM API returned null response");
                return new AnnotationResponse();
            }

            this.Logger.LogInformation("Received response from LLM API with {count} choices",
                response.choices?.Length ?? 0);

            if (response.choices == null || response.choices.Length == 0)
            {
                this.Logger.LogWarning("LLM API returned empty choices");
                return new AnnotationResponse();
            }

            // Extract the content from the response
            var responseContent = response.choices[0].message.content;
            this.Logger.LogInformation("Received content from LLM: {content}", responseContent);

            // Parse the JSON response
            var jsonStartIndex = responseContent.IndexOf('{');
            var jsonEndIndex = responseContent.LastIndexOf('}');

            if (jsonStartIndex >= 0 && jsonEndIndex > jsonStartIndex)
            {
                var jsonString = responseContent.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                this.Logger.LogDebug("Extracted JSON from response: {json}", jsonString);

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

                        // Create a sentence for each unique beginSentence value
                        var uniqueSentenceIndices = quotes.Select(q => q.beginSentence).Distinct().ToList();
                        var sentences = new List<Sentence>();

                        this.Logger.LogDebug("Creating sentences for {count} unique sentence indices", uniqueSentenceIndices.Count);

                        foreach (var index in uniqueSentenceIndices)
                        {
                            this.Logger.LogDebug("Creating sentence for index {index}", index);
                            sentences.Add(new Sentence
                            {
                                index = index,
                                entityMentions = new List<EntityMention>() // Empty entity mentions list
                            });
                        }

                        // Add entity mentions for speakers that are not "Unknown"
                        this.Logger.LogDebug("Adding entity mentions for speakers");
                        foreach (var quote in quotes)
                        {
                            if (quote.canonicalSpeaker != "Unknown")
                            {
                                this.Logger.LogDebug("Adding entity mention for speaker '{speaker}' in sentence {index}",
                                    quote.canonicalSpeaker, quote.beginSentence);

                                // Find or create the sentence for this quote
                                var sentence = sentences.FirstOrDefault(s => s.index == quote.beginSentence);
                                if (sentence == null)
                                {
                                    this.Logger.LogDebug("Creating new sentence for index {index}", quote.beginSentence);
                                    sentence = new Sentence
                                    {
                                        index = quote.beginSentence,
                                        entityMentions = new List<EntityMention>()
                                    };
                                    sentences.Add(sentence);
                                }

                                // Add an entity mention for the speaker
                                sentence.entityMentions.Add(new EntityMention
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
                        this.Logger.LogWarning("Parsed response contains no quotes");
                    }
                }
                catch (JsonException ex)
                {
                    this.Logger.LogError(ex, "Failed to parse LLM response JSON: {json}", jsonString);
                }
            }
            else
            {
                this.Logger.LogWarning("Could not find valid JSON in LLM response. JSON markers: start={start}, end={end}",
                    jsonStartIndex, jsonEndIndex);
            }

            this.Logger.LogWarning("Failed to extract valid JSON from LLM response: {response}", responseContent);
            return new AnnotationResponse();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error performing LLM annotation");
            throw;
        }
    }
    #endregion

    #region Models
    /// <summary>
    /// Model for the LLM API response
    /// </summary>
    private class LLMResponse
    {
        public Choice[] choices { get; set; } = Array.Empty<Choice>();
    }

    /// <summary>
    /// Model for the LLM API response choice
    /// </summary>
    private class Choice
    {
        public Message message { get; set; } = new Message();
    }

    /// <summary>
    /// Model for the LLM API response message
    /// </summary>
    private class Message
    {
        public string content { get; set; } = "";
    }

    /// <summary>
    /// Model for the parsed quote response from the LLM
    /// </summary>
    private class QuoteResponse
    {
        public List<QuoteItem> quotes { get; set; } = new List<QuoteItem>();
    }

    /// <summary>
    /// Model for a quote item in the LLM response
    /// </summary>
    private class QuoteItem
    {
        public int id { get; set; }
        public string text { get; set; } = "";
        public string canonicalSpeaker { get; set; } = "";
        public int beginSentence { get; set; }
    }
    #endregion
}
