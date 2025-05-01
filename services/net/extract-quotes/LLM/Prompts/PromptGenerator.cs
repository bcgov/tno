using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using TNO.API.Areas.Services.Models.Content;
using TNO.Services.ExtractQuotes.Config;

namespace TNO.Services.ExtractQuotes.LLM.Prompts;

/// <summary>
/// Generator for creating prompts for LLM requests, using configuration options.
/// </summary>
public class PromptGenerator : IPromptGenerator
{
    private readonly ILogger<PromptGenerator> _logger;
    private readonly ExtractQuotesOptions _options;
    private readonly string _promptTemplate;

    /// <summary>
    /// Creates a new instance of PromptGenerator.
    /// Validates that the required prompt template is provided in configuration.
    /// </summary>
    /// <param name="options">Service configuration options.</param>
    /// <param name="logger">Logger for this generator.</param>
    /// <exception cref="ArgumentException">Thrown if the prompt template is not configured.</exception>
    public PromptGenerator(IOptions<ExtractQuotesOptions> options, ILogger<PromptGenerator> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Validate required configuration from the main options class now
        if (string.IsNullOrWhiteSpace(_options.QuoteExtractionPromptTemplate))
        {
            // Updated error message to reflect the new location
            throw new ArgumentException("LLM Quote Extraction Prompt Template ('Service:QuoteExtractionPromptTemplate') is missing or empty in configuration.", nameof(options));
        }
        if (!_options.QuoteExtractionPromptTemplate.Contains("{InputText}", StringComparison.OrdinalIgnoreCase))
        {
            // Updated the exception message as requested in the previous step
            throw new ArgumentException("The configured LLM Quote Extraction Prompt Template must contain the required placeholder '{InputText}' for text replacement.", nameof(options));
        }
        if (!_options.QuoteExtractionPromptTemplate.Contains("{ExistingQuotes}", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("The configured LLM Quote Extraction Prompt Template does not contain the 'ExistingQuotes' placeholder. Existing quotes will not be excluded from extraction.");
        }

        _promptTemplate = _options.QuoteExtractionPromptTemplate; // Read directly from options
        _logger.LogInformation("PromptGenerator initialized using prompt template from configuration.");
    }

    /// <summary>
    /// Generates a prompt for extracting quotes from text using the configured template.
    /// </summary>
    /// <param name="text">The text to extract quotes from.</param>
    /// <param name="existingQuotes">Optional list of existing quotes to exclude from extraction.</param>
    /// <returns>A formatted prompt string.</returns>
    public string GenerateQuoteExtractionPrompt(string text, IEnumerable<QuoteModel>? existingQuotes = null)
    {
        _logger.LogDebug("Generating quote extraction prompt for text of length {length}", text.Length);

        // Format existing quotes if provided
        string existingQuotesText = "None";
        if (existingQuotes != null && existingQuotes.Any())
        {
            var sb = new StringBuilder();
            foreach (var quote in existingQuotes)
            {
                sb.AppendLine($"Speaker: {quote.Byline}");
                sb.AppendLine($"Quote: \"{quote.Statement}\"");
                sb.AppendLine();
            }
            existingQuotesText = sb.ToString().TrimEnd();
            _logger.LogDebug("Including {count} existing quotes in prompt", existingQuotes.Count());
        }

        // Replace the placeholders with the actual text and existing quotes
        string finalPrompt = _promptTemplate
            .Replace("{InputText}", text, StringComparison.OrdinalIgnoreCase)
            .Replace("{ExistingQuotes}", existingQuotesText, StringComparison.OrdinalIgnoreCase);

        return finalPrompt;
    }
}
