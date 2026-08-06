#pragma warning disable OPENAI001

using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;
using TNO.AI.Models;

namespace TNO.AI;

/// <summary>
/// AIAgentClient class, executes prompts against Azure AI Foundry agents.
/// </summary>
public class AIAgentClient : IAIAgentClient
{
    #region Variables
    private readonly ILogger<AIAgentClient> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AIAgentClient object.
    /// </summary>
    /// <param name="logger"></param>
    public AIAgentClient(ILogger<AIAgentClient> logger)
    {
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Execute an analysis request against a Foundry agent and return response text.
    /// Automatically approves MCP tool calls as Foundry requires explicit approval.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> AnalyzeAsync(AIAgentRequestModel request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AgentName)) throw new ArgumentException("AgentName is required.", nameof(request));
        if (request.ProjectEndpoint == null) throw new ArgumentException("ProjectEndpoint is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Prompt)) throw new ArgumentException("Prompt is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.TenantId) ||
            string.IsNullOrWhiteSpace(request.ClientId) ||
            string.IsNullOrWhiteSpace(request.ClientSecret))
            throw new InvalidOperationException("Azure AI credentials are not configured (TenantId, ClientId, ClientSecret).");

        var credential = new ClientSecretCredential(
            tenantId: request.TenantId,
            clientId: request.ClientId,
            clientSecret: request.ClientSecret);

        var projectClient = new AIProjectClient(
            endpoint: request.ProjectEndpoint,
            tokenProvider: credential);

        var agentReference = new AgentReference(name: request.AgentName);
        var responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(agentReference);

        ResponseResult response = await Task.Run(
            () => responseClient.CreateResponse(request.Prompt),
            cancellationToken);

        _logger.LogDebug("AI agent: Model={Model} Status={Status} Error={Error}",
            response.Model, response.Status, response.Error?.Message);

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

            var continueOptions = new CreateResponseOptions(request.DeploymentName ?? string.Empty, approvals)
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
