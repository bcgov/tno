using TNO.AI;
using TNO.AI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.TemplateEngine.Config;

namespace TNO.TemplateEngine;

/// <summary>
/// AIAgentService class, sends prompts to the configured Azure AI Foundry agent.
/// </summary>
public class AIAgentService : IAIAgentService
{
    #region Variables
    private readonly AzureAIOptions? _options;
    private readonly ILogger<AIAgentService> _logger;
    private readonly IAIAgentClient _client;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AIAgentService object.
    /// </summary>
    public AIAgentService(IOptions<AzureOptions> azureOptions, IAIAgentClient client, ILogger<AIAgentService> logger)
    {
        _options = azureOptions.Value.AI;
        _client = client;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Send a prompt to the Azure AI Foundry agent and return the text response.
    /// Credentials are read from configuration; agentName and projectEndpoint are supplied per-call.
    /// Automatically approves MCP tool calls as Azure Foundry requires explicit approval.
    /// </summary>
    public async Task<string> AnalyzeAsync(
        string agentName,
        Uri projectEndpoint,
        string prompt,
        string? deploymentName = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Sending request to Foundry agent '{AgentName}'", agentName);

        var request = new AIAgentRequestModel
        {
            AgentName = agentName,
            ProjectEndpoint = projectEndpoint,
            Prompt = prompt,
            DeploymentName = deploymentName,
            TenantId = _options?.TenantId,
            ClientId = _options?.ClientId,
            ClientSecret = _options?.ClientSecret,
        };

        return await _client.AnalyzeAsync(request, cancellationToken);
    }
    #endregion
}
