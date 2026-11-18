namespace TNO.API.Models.FilterAI;

/// <summary>
/// FilterAIChatResultModel class, the answer to a question about a filter prompt.
/// </summary>
public class FilterAIChatResultModel
{
    #region Properties
    /// <summary>
    /// get/set - The answer, as plain text or markdown.
    /// </summary>
    public string Answer { get; set; } = "";

    /// <summary>
    /// get/set - A complete replacement prompt as HTML, when the model proposed one.
    /// </summary>
    public string? SuggestedPrompt { get; set; }

    /// <summary>
    /// get/set - The fence nonce of the conversation, to send with the next question.
    /// </summary>
    public string Nonce { get; set; } = "";

    /// <summary>
    /// get/set - The conversation so far (user and assistant turns), to send with the next question.
    /// </summary>
    public IEnumerable<FilterAIMessageModel> Messages { get; set; } = Array.Empty<FilterAIMessageModel>();

    /// <summary>
    /// get/set - The tools the model used to answer, in order.
    /// </summary>
    public IEnumerable<FilterAIStepModel> Steps { get; set; } = Array.Empty<FilterAIStepModel>();
    #endregion
}
