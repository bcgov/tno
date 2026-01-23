namespace TNO.TemplateEngine.Config;

/// <summary>
/// AzureAIOptions class, provides a way to configure Azure AI settings
/// </summary>
public class AzureAIOptions
{
    #region Properties
    /// <summary>
    /// get/set - The URL to the Azure AI API
    /// </summary>
    public Uri ProjectEndpoint { get; set; } = default!;

    /// <summary>
    /// get/set - The API key.
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// get/set - Name of the model deployment (i.e. gpt-5.1-chat)
    /// </summary>
    public string DefaultModelDeploymentName { get; set; } = "";

    /// <summary>
    /// get/set - Name of the deployed agent
    /// </summary>
    public string? DefaultAgentName { get; set; }

    /// <summary>
    /// get/set - Default system prompt for report AI summary.
    /// </summary>
    public string? DefaultSystemPrompt { get; set; }

    /// <summary>
    /// get/set - Default user prompt for report AI summary.
    /// </summary>
    public string? DefaultUserPrompt { get; set; }
    #endregion
}
