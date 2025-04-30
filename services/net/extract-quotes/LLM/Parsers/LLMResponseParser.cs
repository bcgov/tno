using System.Text.Json;
using Microsoft.Extensions.Logging;
using TNO.Services.ExtractQuotes.CoreNLP.models;
using TNO.Services.ExtractQuotes.LLM.Models;

namespace TNO.Services.ExtractQuotes.LLM.Parsers;

/// <summary>
/// Parser for LLM responses that extracts quotes and converts them to the expected format
/// </summary>
public class LLMResponseParser : ILLMResponseParser
{
    private readonly ILogger<LLMResponseParser> _logger;

    /// <summary>
    /// Creates a new instance of LLMResponseParser
    /// </summary>
    /// <param name="logger">Logger for this parser</param>
    public LLMResponseParser(ILogger<LLMResponseParser> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses the raw string content from the LLM response into an AnnotationResponse
    /// </summary>
    /// <param name="llmResponseContent">The string content from the LLM response</param>
    /// <param name="modelName">The name of the model that generated the response (for logging)</param>
    /// <returns>AnnotationResponse if parsing is successful, otherwise null</returns>
    public AnnotationResponse? ParseLLMResponse(string llmResponseContent, string modelName)
    {
        // Extract JSON from the response
        int jsonStartIndex = llmResponseContent.IndexOf('{');
        int jsonEndIndex = llmResponseContent.LastIndexOf('}');

        if (jsonStartIndex >= 0 && jsonEndIndex > jsonStartIndex)
        {
            var jsonString = llmResponseContent.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);

            try
            {
                // Deserialize the JSON to QuoteResponse
                var quoteResponse = JsonSerializer.Deserialize<QuoteResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (quoteResponse != null && quoteResponse.quotes != null && quoteResponse.quotes.Count > 0)
                {
                    _logger.LogInformation("Successfully parsed {count} quotes from LLM response (Model: {Model})",
                        quoteResponse.quotes.Count, modelName);

                    // Convert QuoteResponse to AnnotationResponse
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
                    _logger.LogDebug("Created {count} sentences structure based on quote indices.", sentences.Count);

                    var annotationResponse = new AnnotationResponse
                    {
                        Quotes = quotes,
                        Sentences = sentences
                    };

                    // Log each extracted quote for debugging
                    foreach (var quote in annotationResponse.Quotes)
                    {
                        _logger.LogDebug("Extracted quote: Speaker='{speaker}', Text='{text}'",
                            quote.canonicalSpeaker, quote.text);
                    }

                    return annotationResponse;
                }
                else
                {
                    _logger.LogWarning("Parsed JSON from model '{Model}' does not contain valid quotes or is empty. JSON: {json}", modelName, jsonString);
                    return null;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse LLM response JSON from model '{Model}'. JSON: {json}", modelName, jsonString);
                return null;
            }
        }
        else
        {
            _logger.LogWarning("Unable to extract JSON object from LLM response content from model '{Model}'. Content: {content}", modelName, llmResponseContent);
            return null;
        }
    }
}
