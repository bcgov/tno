namespace TNO.Services.Automation.Config;

/// <summary>
/// AzureAIOptions class, credentials used to authenticate with Azure AI Foundry.
/// The agent name, project endpoint, and deployment come from the profile's configured LLM.
/// </summary>
public class AzureAIOptions
{
    #region Properties
    /// <summary>
    /// get/set - Azure tenant id.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// get/set - Azure client id.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// get/set - Azure client secret.
    /// </summary>
    public string? ClientSecret { get; set; }
    #endregion
}
