namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// LlmMessageModel class, one message of a request to a model.
/// </summary>
public class LlmMessageModel
{
    #region Properties
    /// <summary>get/set - 'system', 'user' or 'assistant'.</summary>
    public string Role { get; set; } = "user";

    /// <summary>get/set - The text.</summary>
    public string Content { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LlmMessageModel.
    /// </summary>
    public LlmMessageModel() { }

    /// <summary>
    /// Creates a new instance of a LlmMessageModel, initializes with specified parameters.
    /// </summary>
    /// <param name="role"></param>
    /// <param name="content"></param>
    public LlmMessageModel(string role, string content)
    {
        this.Role = role;
        this.Content = content;
    }
    #endregion
}
