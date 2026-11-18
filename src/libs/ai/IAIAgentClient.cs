using TNO.AI.Models;

namespace TNO.AI;

/// <summary>
/// IAIAgentClient interface, provides a generic way to call an Azure AI Foundry agent.
/// </summary>
public interface IAIAgentClient
{
    /// <summary>
    /// Execute an analysis request against a Foundry agent and return response text.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> AnalyzeAsync(AIAgentRequestModel request, CancellationToken cancellationToken = default);
}
