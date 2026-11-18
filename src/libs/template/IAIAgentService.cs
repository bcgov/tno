namespace TNO.TemplateEngine;

/// <summary>
/// IAIAgentService interface, provides a way to interact with the configured Azure AI Foundry agent.
/// </summary>
public interface IAIAgentService
{
    /// <summary>
    /// Send a prompt to the Azure AI Foundry agent and return the text response.
    /// </summary>
    /// <param name="agentName">Name of the Foundry agent to invoke.</param>
    /// <param name="projectEndpoint">Azure AI Foundry project endpoint URI.</param>
    /// <param name="prompt">The full prompt to send (caller combines system + user text as needed).</param>
    /// <param name="deploymentName">Model deployment name used for MCP continuation options.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The agent's text response.</returns>
    Task<string> AnalyzeAsync(
        string agentName,
        Uri projectEndpoint,
        string prompt,
        string? deploymentName = null,
        CancellationToken cancellationToken = default);
}
