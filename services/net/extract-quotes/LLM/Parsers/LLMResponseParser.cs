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
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new instance of LLMResponseParser
    /// </summary>
    /// <param name="logger">Logger for this parser</param>
    public LLMResponseParser(ILogger<LLMResponseParser> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    /// <summary>
    /// Parses the raw string content from the LLM response into an AnnotationResponse
    /// </summary>
    /// <param name="llmResponseContent">The string content from the LLM response</param>
    /// <param name="modelName">The name of the model that generated the response (for logging)</param>
    /// <returns>AnnotationResponse if parsing is successful, otherwise null</returns>
    public AnnotationResponse? ParseLLMResponse(string llmResponseContent, string modelName)
    {
        try
        {
            // Directly deserialize the JSON response
            var quoteResponse = JsonSerializer.Deserialize<QuoteResponse>(llmResponseContent, _jsonOptions);

            // Check if deserialization was successful and if the quotes property exists
            if (quoteResponse != null && quoteResponse.quotes != null)
            {
                int quoteCount = quoteResponse.quotes.Count;

                if (quoteCount > 0)
                {
                    _logger.LogInformation("Successfully parsed {count} quotes from LLM response (Model: {Model})",
                        quoteCount, modelName);
                }
                else
                {
                    _logger.LogInformation("Model '{Model}' returned valid JSON with empty quotes array", modelName);
                }

                return CreateAnnotationResponse(quoteResponse);
            }
            else
            {
                _logger.LogWarning("Parsed JSON from model '{Model}' does not contain valid quotes property. JSON: {json}", modelName, llmResponseContent);
                return null;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse LLM response JSON from model '{Model}'. Content: {content}", modelName, llmResponseContent);
            return null;
        }
    }

    /// <summary>
    /// Creates an AnnotationResponse from a QuoteResponse
    /// </summary>
    /// <param name="quoteResponse">The parsed QuoteResponse</param>
    /// <returns>An AnnotationResponse object</returns>
    private AnnotationResponse CreateAnnotationResponse(QuoteResponse quoteResponse)
    {
        // Handle case where no quotes were found
        if (quoteResponse.quotes.Count == 0)
        {
            _logger.LogDebug("Creating empty AnnotationResponse (no quotes found)");
            return new AnnotationResponse
            {
                Quotes = new List<Quote>(),
                Sentences = new List<Sentence>()
            };
        }

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
                            .Select(i => new Sentence
                            {
                                index = i,
                                entityMentions = [] // Use collection expression for empty list
                            })
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
}
