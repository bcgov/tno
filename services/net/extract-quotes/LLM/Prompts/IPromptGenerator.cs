namespace TNO.Services.ExtractQuotes.LLM.Prompts;

/// <summary>
/// Interface for generating prompts for LLM requests
/// </summary>
public interface IPromptGenerator
{
    /// <summary>
    /// Generates a prompt for extracting quotes from text
    /// </summary>
    /// <param name="text">The text to extract quotes from</param>
    /// <returns>A formatted prompt string</returns>
    string GenerateQuoteExtractionPrompt(string text);
}
