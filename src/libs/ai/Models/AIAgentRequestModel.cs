namespace TNO.AI.Models;

/// <summary>
/// AIAgentRequestModel class, contains all information required to call a Foundry agent.
/// </summary>
public class AIAgentRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - Name of the Foundry agent.
    /// </summary>
    public string AgentName { get; set; } = "";

    /// <summary>
    /// get/set - Foundry project endpoint.
    /// </summary>
    public Uri ProjectEndpoint { get; set; } = default!;

    /// <summary>
    /// get/set - Prompt content to send to the agent.
    /// </summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// get/set - Optional deployment name used for continuation calls.
    /// </summary>
    public string? DeploymentName { get; set; }

    /// <summary>
    /// get/set - Azure tenant id for service principal auth.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// get/set - Azure client id for service principal auth.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// get/set - Azure client secret for service principal auth.
    /// </summary>
    public string? ClientSecret { get; set; }
    #endregion
}
