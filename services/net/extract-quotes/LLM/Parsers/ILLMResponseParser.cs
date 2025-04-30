using TNO.Services.ExtractQuotes.CoreNLP.models;

namespace TNO.Services.ExtractQuotes.LLM.Parsers;

/// <summary>
/// Interface for parsing LLM responses into structured data
/// </summary>
public interface ILLMResponseParser
{
    /// <summary>
    /// Parses the raw string content from the LLM response into an AnnotationResponse
    /// </summary>
    /// <param name="llmResponseContent">The string content from the LLM response</param>
    /// <param name="modelName">The name of the model that generated the response (for logging)</param>
    /// <returns>AnnotationResponse if parsing is successful, otherwise null</returns>
    AnnotationResponse? ParseLLMResponse(string llmResponseContent, string modelName);
}
