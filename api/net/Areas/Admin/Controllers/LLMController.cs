using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.LLM;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Keycloak;
using TNO.TemplateEngine.Config;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// LLMController class, provides LLM endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/llms")]
[Route("api/[area]/llms")]
[Route("v{version:apiVersion}/[area]/llms")]
[Route("[area]/llms")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class LLMController : ControllerBase
{
    #region Variables
    private readonly ILLMService _service;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly AzureOptions _azureOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LLMController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="azureOptions"></param>
    public LLMController(ILLMService service, IOptions<JsonSerializerOptions> serializerOptions, IOptions<AzureOptions> azureOptions)
    {
        _service = service;
        _serializerOptions = serializerOptions.Value;
        _azureOptions = azureOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of LLMS for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<LLMModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "LLM" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new LLMModel(ds)));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LLMModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "LLM" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new LLMModel(result));
    }

    /// <summary>
    /// Find everything that uses the LLM connection for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/usage")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LLMUsageModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "LLM" })]
    public IActionResult FindUsage(int id)
    {
        return new JsonResult(_service.FindUsage(id));
    }

    /// <summary>
    /// Check whether everything using the LLM connection can move to the target connection.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    /// <returns></returns>
    [HttpGet("{id}/migrate/{targetId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LLMMigrationCheckModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "LLM" })]
    public IActionResult CheckMigration(int id, int targetId)
    {
        return new JsonResult(_service.CheckMigration(id, targetId, HasDefaultApiKey, HasDefaultDeployment));
    }

    /// <summary>
    /// Move everything using the LLM connection to the target connection.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    /// <returns></returns>
    [HttpPost("{id}/migrate/{targetId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LLMMigrationResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "LLM" })]
    public IActionResult Migrate(int id, int targetId)
    {
        return new JsonResult(_service.Migrate(id, targetId, HasDefaultApiKey, HasDefaultDeployment));
    }

    /// <summary>
    /// Add content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LLMModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "LLM" })]
    public IActionResult Add([FromBody] LLMModel model)
    {
        ValidateLimits(model);
        var result = _service.AddAndSave(model.ToEntity());
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new LLMModel(result));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LLMModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "LLM" })]
    public IActionResult Update([FromBody] LLMModel model)
    {
        ValidateLimits(model);
        var result = _service.UpdateAndSave(model.ToEntity());
        return new JsonResult(new LLMModel(result));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LLMModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "LLM" })]
    public IActionResult Delete([FromBody] LLMModel model)
    {
        _service.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }
    #endregion

    #region Methods
    private bool HasDefaultApiKey => !String.IsNullOrWhiteSpace(_azureOptions.AI?.ApiKey);
    private bool HasDefaultDeployment => !String.IsNullOrWhiteSpace(_azureOptions.AI?.DefaultModelDeploymentName);

    /// <summary>
    /// Validate the token and rate limits a direct-model LLM needs for bounded requests.
    /// </summary>
    /// <param name="model"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void ValidateLimits(LLMModel model)
    {
        var errors = new List<string>();
        if (model.ContextWindow <= 0) errors.Add("The context window must be greater than 0.");
        if (model.MaxOutputTokens <= 0) errors.Add("The maximum output tokens must be greater than 0.");
        if (model.ContextWindow.HasValue && model.MaxOutputTokens.HasValue && model.MaxOutputTokens >= model.ContextWindow)
            errors.Add("The maximum output tokens must be less than the context window.");
        if (!String.IsNullOrWhiteSpace(model.TokenEstimation) && !TNO.AI.Tokens.TokenEstimator.IsSupported(model.TokenEstimation))
            errors.Add($"The token estimation must be one of: {String.Join(", ", TNO.AI.Tokens.TokenEstimationStrategy.All)}.");
        if (model.RequestsPerMinute < 0) errors.Add("Requests per minute cannot be negative.");
        if (model.TokensPerMinute < 0) errors.Add("Tokens per minute cannot be negative.");
        if (errors.Count > 0) throw new ArgumentException(String.Join(" ", errors));
    }
    #endregion
}
