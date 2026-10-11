namespace TNO.API.Areas.Subscriber.Models.Report;

/// <summary>
/// ReportAIPromptChatResultModel class, the model's answer, a prompt it proposes when it does, and
/// the conversation to send back on the next turn.
/// </summary>
public class ReportAIPromptChatResultModel
{
    #region Properties
    /// <summary>get/set - The composed first-turn context (empty on later turns), so the user can see what was sent.</summary>
    public string Prompt { get; set; } = "";

    /// <summary>get/set - The answer.</summary>
    public string Answer { get; set; } = "";

    /// <summary>get/set - A complete replacement prompt as HTML, when the model proposes one.</summary>
    public string? SuggestedPrompt { get; set; }

    /// <summary>get/set - The conversation including this turn.</summary>
    public IEnumerable<ReportAIPromptChatMessageModel> Messages { get; set; } = Array.Empty<ReportAIPromptChatMessageModel>();
    #endregion
}
