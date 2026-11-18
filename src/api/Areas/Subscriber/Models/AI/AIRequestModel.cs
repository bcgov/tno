namespace TNO.API.Areas.Subscriber.Models.AI;

/// <summary>
/// AIRequestModel class, model for AI analysis requests.
/// </summary>
public class AIRequestModel
{
    /// <summary>
    /// get/set - The prompt to send to the AI agent.
    /// </summary>
    public string Prompt { get; set; } = "";
}
