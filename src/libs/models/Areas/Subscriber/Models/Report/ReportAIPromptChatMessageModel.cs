namespace TNO.API.Areas.Subscriber.Models.Report;

/// <summary>
/// ReportAIPromptChatMessageModel class, one message in a conversation about an AI section prompt.
/// </summary>
public class ReportAIPromptChatMessageModel
{
    #region Properties
    /// <summary>get/set - 'system', 'user' or 'assistant'.</summary>
    public string Role { get; set; } = "user";

    /// <summary>get/set - The text.</summary>
    public string Content { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAIPromptChatMessageModel.
    /// </summary>
    public ReportAIPromptChatMessageModel() { }

    /// <summary>
    /// Creates a new instance of a ReportAIPromptChatMessageModel, initializes with specified parameters.
    /// </summary>
    /// <param name="role"></param>
    /// <param name="content"></param>
    public ReportAIPromptChatMessageModel(string role, string content)
    {
        this.Role = role;
        this.Content = content;
    }
    #endregion
}
