namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationDebugMessageModel class, a single message in a debugging conversation.
/// </summary>
public class AutomationDebugMessageModel
{
    #region Properties
    /// <summary>
    /// get/set - The role of the message author ("system", "user", or "assistant").
    /// </summary>
    public string Role { get; set; } = "user";

    /// <summary>
    /// get/set - The message content.
    /// </summary>
    public string Content { get; set; } = "";
    #endregion

    #region Constructors
    public AutomationDebugMessageModel() { }

    public AutomationDebugMessageModel(string role, string content)
    {
        this.Role = role;
        this.Content = content;
    }
    #endregion
}

/// <summary>
/// AutomationDebugRequestModel class, a request to ask the profile's LLM why a specific content item
/// was (or was not) acted upon. Supports a continued conversation: the first turn has an empty
/// <see cref="Messages"/> list (the server composes the content/run context), and follow-up turns
/// pass the conversation returned from the previous response back so the LLM retains context.
/// </summary>
public class AutomationDebugRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - The content item to inquire about (used to compose the context on the first turn).
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The user's message (e.g. "Why was the following content item not published?" on the
    /// first turn, or a follow-up question on later turns).
    /// </summary>
    public string Question { get; set; } = "";

    /// <summary>
    /// get/set - The conversation so far, as returned by the previous response. Empty starts a new chat.
    /// </summary>
    public IEnumerable<AutomationDebugMessageModel> Messages { get; set; } = Array.Empty<AutomationDebugMessageModel>();
    #endregion
}

/// <summary>
/// AutomationDebugResultModel class, the LLM's answer plus the full conversation to send back on the
/// next turn, and (on the first turn) the composed prompt so the user can see exactly what was sent.
/// </summary>
public class AutomationDebugResultModel
{
    #region Properties
    /// <summary>
    /// get/set - The content item that was inquired about.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The id of the last successful run whose information was included, if any.
    /// </summary>
    public long? RunId { get; set; }

    /// <summary>
    /// get/set - The composed first-turn prompt (empty on follow-up turns).
    /// </summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// get/set - The LLM's answer to this turn.
    /// </summary>
    public string Answer { get; set; } = "";

    /// <summary>
    /// get/set - The full conversation (including this turn's answer) to pass back on the next turn.
    /// </summary>
    public IEnumerable<AutomationDebugMessageModel> Messages { get; set; } = Array.Empty<AutomationDebugMessageModel>();
    #endregion
}
