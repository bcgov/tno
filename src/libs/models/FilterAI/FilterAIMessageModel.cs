namespace TNO.API.Models.FilterAI;

/// <summary>
/// FilterAIMessageModel class, one turn of a filter AI conversation.
/// </summary>
public class FilterAIMessageModel
{
    #region Properties
    /// <summary>
    /// get/set - The role of the turn [user, assistant].
    /// </summary>
    public string Role { get; set; } = "user";

    /// <summary>
    /// get/set - The content of the turn.
    /// </summary>
    public string Content { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FilterAIMessageModel.
    /// </summary>
    public FilterAIMessageModel() { }

    /// <summary>
    /// Creates a new instance of a FilterAIMessageModel, initializes with specified parameters.
    /// </summary>
    /// <param name="role"></param>
    /// <param name="content"></param>
    public FilterAIMessageModel(string role, string content)
    {
        this.Role = role;
        this.Content = content;
    }
    #endregion
}
