using TNO.Services.ExtractQuotes.Config;
using TNO.Services.ExtractQuotes.LLM.Models;

namespace TNO.Services.ExtractQuotes.LLM.Clients;

/// <summary>
/// Interface for LLM client that handles API calls to LLM providers
/// </summary>
public interface ILLMClient
{
    /// <summary>
    /// Sends a prompt to the LLM API and returns the raw response content
    /// </summary>
    /// <param name="text">The original text to be processed</param>
    /// <param name="prompt">The prompt to send to the LLM</param>
    /// <param name="modelName">The name of the model to use</param>
    /// <param name="apiKey">The API key to use for authentication</param>
    /// <returns>The raw response content from the LLM API, or null if the request failed</returns>
    Task<string?> CallLLMApiWithPrompt(string text, string prompt, string modelName, string apiKey);

    /// <summary>
    /// Gets the next API key from the list in a thread-safe, round-robin manner
    /// </summary>
    /// <param name="keys">List of API keys</param>
    /// <param name="indexField">Reference to the index field for this list</param>
    /// <returns>The next API key</returns>
    string GetNextApiKey(List<string> keys, ref int indexField);

    /// <summary>
    /// Gets the next API key from a semicolon-separated string in a thread-safe, round-robin manner
    /// </summary>
    /// <param name="apiKeysString">Semicolon-separated string of API keys</param>
    /// <param name="indexField">Reference to the index field for this list</param>
    /// <returns>The next API key</returns>
    string GetNextApiKeyFromString(string apiKeysString, ref int indexField);
}
