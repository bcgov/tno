using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.AI;
using TNO.API.Models;
using TNO.Keycloak;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Config;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// AIController class, provides AI analysis endpoints for the subscriber area.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/ai")]
[Route("api/[area]/ai")]
[Route("v{version:apiVersion}/[area]/ai")]
[Route("[area]/ai")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class AIController : ControllerBase
{
    #region Variables
    private readonly IAIAgentService _aiAgent;
    private readonly AzureAIOptions? _azureAI;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AIController object, initializes with specified parameters.
    /// </summary>
    /// <param name="aiAgent"></param>
    /// <param name="azureOptions"></param>
    public AIController(IAIAgentService aiAgent, IOptions<AzureOptions> azureOptions)
    {
        _aiAgent = aiAgent;
        _azureAI = azureOptions.Value.AI;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Send a prompt to the configured Azure AI Foundry agent and return the response.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("analyze")]
    [Produces(MediaTypeNames.Text.Plain)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "AI" })]
    public async Task<IActionResult> Analyze(
        [FromBody] AIRequestModel model,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Prompt))
            return BadRequest(new ErrorResponseModel("Prompt is required.", ""));

        if (_azureAI?.ProjectEndpoint == null)
            return BadRequest(new ErrorResponseModel("Azure AI project endpoint is not configured.", ""));

        if (string.IsNullOrWhiteSpace(_azureAI.DefaultAgentName))
            return BadRequest(new ErrorResponseModel("Azure AI default agent name is not configured.", ""));

        var prompt = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(_azureAI.DefaultSystemPrompt))
            prompt.AppendLine(_azureAI.DefaultSystemPrompt);
        prompt.Append(model.Prompt);

        try
        {
            var result = await _aiAgent.AnalyzeAsync(
                agentName: _azureAI.DefaultAgentName,
                projectEndpoint: _azureAI.ProjectEndpoint,
                prompt: prompt.ToString(),
                deploymentName: _azureAI.DefaultModelDeploymentName,
                cancellationToken: cancellationToken);

            return Content(result, MediaTypeNames.Text.Plain);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponseModel(ex.Message, ""));
        }
    }
    #endregion
}
