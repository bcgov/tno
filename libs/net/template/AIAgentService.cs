#pragma warning disable OPENAI001

using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AIAgentService object.
    /// </summary>
    public AIAgentService(IOptions<AzureOptions> azureOptions, ILogger<AIAgentService> logger)
    {
        _options = azureOptions.Value.AI;
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
        if (string.IsNullOrWhiteSpace(_options?.TenantId) ||
            string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret))
            throw new InvalidOperationException("Azure AI credentials are not configured (TenantId, ClientId, ClientSecret).");

        var credential = new ClientSecretCredential(
            tenantId: _options.TenantId,
            clientId: _options.ClientId,
            clientSecret: _options.ClientSecret);

        var projectClient = new AIProjectClient(
            endpoint: projectEndpoint,
            tokenProvider: credential);

        var agentReference = new AgentReference(name: agentName);
        var responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(agentReference);

        ResponseResult response = await Task.Run(
            () => responseClient.CreateResponse(prompt),
            cancellationToken);

        _logger.LogDebug("AI agent: Model={Model} Status={Status} Error={Error}",
            response.Model, response.Status, response.Error?.Message);

        // Azure Foundry requires explicit approval for MCP tool calls.
        var maxIterations = 10;
        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var approvalRequests = response.OutputItems
                .OfType<McpToolCallApprovalRequestItem>()
                .ToList();
            if (approvalRequests.Count == 0) break;

            _logger.LogDebug("AI agent: approving {Count} MCP tool call(s)", approvalRequests.Count);
            var approvals = approvalRequests
                .Select(r => ResponseItem.CreateMcpApprovalResponseItem(r.Id, approved: true))
                .ToList<ResponseItem>();

            var continueOptions = new CreateResponseOptions(deploymentName ?? string.Empty, approvals)
            {
                PreviousResponseId = response.Id
            };
            response = await Task.Run(
                () => responseClient.CreateResponse(continueOptions),
                cancellationToken);

            _logger.LogDebug("AI agent continuation: Status={Status} Error={Error}",
                response.Status, response.Error?.Message);
        }

        return response.GetOutputText() ?? string.Empty;
    }
    #endregion
}
