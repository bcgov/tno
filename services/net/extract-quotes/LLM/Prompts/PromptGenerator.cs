using Microsoft.Extensions.Logging;

namespace TNO.Services.ExtractQuotes.LLM.Prompts;

/// <summary>
/// Generator for creating prompts for LLM requests
/// </summary>
public class PromptGenerator : IPromptGenerator
{
    private readonly ILogger<PromptGenerator> _logger;

    /// <summary>
    /// Creates a new instance of PromptGenerator
    /// </summary>
    /// <param name="logger">Logger for this generator</param>
    public PromptGenerator(ILogger<PromptGenerator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates a prompt for extracting quotes from text
    /// </summary>
    /// <param name="text">The text to extract quotes from</param>
    /// <returns>A formatted prompt string</returns>
    public string GenerateQuoteExtractionPrompt(string text)
    {
        _logger.LogDebug("Generating quote extraction prompt for text of length {length}", text.Length);

        return $@"Extract all direct quotes from the following text. For each quote, identify the speaker.
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
    }
}
