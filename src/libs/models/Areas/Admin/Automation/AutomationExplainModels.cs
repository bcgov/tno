namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationExplainRequestModel class, a request to open (or continue) an explain-and-improve
/// conversation about one run log entry. The first turn is seeded by the API with the entry's
/// prompt, response, parsed outcome, action configuration, and content digest; follow-up turns
/// carry the conversation back.
/// </summary>
public class AutomationExplainRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - The user's question (e.g. "why did this not confirm?", "how do I improve this prompt?").
    /// </summary>
    public string Question { get; set; } = "";

    /// <summary>
    /// get/set - The conversation so far; empty on the first turn.
    /// </summary>
    public IEnumerable<AutomationDebugMessageModel> Messages { get; set; } = Array.Empty<AutomationDebugMessageModel>();
    #endregion
}

/// <summary>
/// AutomationExplainResultModel class, the assistant's answer. When the assistant proposes a
/// prompt revision it is extracted into SuggestedPrompt so the editor can show it as a diff;
/// a revision is only ever applied by an explicit admin save.
/// </summary>
public class AutomationExplainResultModel
{
    #region Properties
    /// <summary>
    /// get/set - The log entry the conversation is about.
    /// </summary>
    public long LogId { get; set; }

    /// <summary>
    /// get/set - The assistant's answer.
    /// </summary>
    public string Answer { get; set; } = "";

    /// <summary>
    /// get/set - A proposed prompt revision extracted from the answer, when the assistant made one.
    /// </summary>
    public string? SuggestedPrompt { get; set; }

    /// <summary>
    /// get/set - The full conversation to send back on the next turn.
    /// </summary>
    public IEnumerable<AutomationDebugMessageModel> Messages { get; set; } = Array.Empty<AutomationDebugMessageModel>();
    #endregion
}
