using TNO.API.Models.FilterAI;

namespace TNO.API.Helpers;

/// <summary>
/// IFilterAIHelper interface, generates filter settings from a prompt and answers questions about
/// how to write that prompt, with the AI model an administrator chose for filters.
/// </summary>
public interface IFilterAIHelper
{
    /// <summary>
    /// The name of the setting holding the id of the AI model used for filters.
    /// </summary>
    const string LLMSettingName = "FilterAILlmId";

    /// <summary>
    /// Generate filter settings from the prompt.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="forSubscriber">Search only published content the current user may see.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FilterAIGenerateResultModel> GenerateAsync(FilterAIGenerateRequestModel request, bool forSubscriber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Answer a question about how to write the filter prompt.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="forSubscriber">Search only published content the current user may see.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FilterAIChatResultModel> ChatAsync(FilterAIChatRequestModel request, bool forSubscriber, CancellationToken cancellationToken = default);
}
