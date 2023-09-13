using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.AVOverview;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// AVOverviewController class, provides endpoints to manage evening overviews.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/reports/av/evening-overview")]
[Route("api/[area]/reports/av/evening-overview")]
[Route("v{version:apiVersion}/[area]/reports/av/evening-overview")]
[Route("[area]/reports/av/evening-overview")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class AVOverviewController : ControllerBase
{
    #region Variables
    private readonly IAVOverviewInstanceService _overviewInstanceService;
    private readonly IAVOverviewTemplateService _overviewTemplateInstanceService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewController object, initializes with specified parameters.
    /// </summary>
    /// <param name="overviewInstanceService"></param>
    /// <param name="overviewTemplateInstanceService"></param>
    /// <param name="serializerOptions"></param>
    public AVOverviewController(
        IAVOverviewInstanceService overviewInstanceService,
        IAVOverviewTemplateService overviewTemplateInstanceService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _overviewInstanceService = overviewInstanceService;
        _overviewTemplateInstanceService = overviewTemplateInstanceService;
        _serializerOptions = serializerOptions.Value;

    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find evening overview instance.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindInstance(int id)
    {
        var instance = _overviewInstanceService.FindById(id);
        if (instance == null) return new NoContentResult();
        var template = _overviewTemplateInstanceService.FindById(instance.TemplateType);
        instance.Template = template;
        return new JsonResult(new AVOverviewInstanceModel(instance, _serializerOptions));
    }

    /// <summary>
    /// Update evening overview for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Update(AVOverviewInstanceModel model)
    {
        var result = _overviewInstanceService.UpdateAndSave((Entities.AVOverviewInstance)model);
        var instance = _overviewInstanceService.FindById(result.Id) ?? throw new InvalidOperationException("Overview Section does not exist");
        return new JsonResult(new AVOverviewInstanceModel(instance, _serializerOptions));
    }
    #endregion
}
