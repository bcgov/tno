using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.AVOverview;
using TNO.API.Models;
using TNO.Core.Exceptions;
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
[Route("api/v{version:apiVersion}/[area]/reports/av/overviews")]
[Route("api/[area]/reports/av/overviews")]
[Route("v{version:apiVersion}/[area]/reports/av/overviews")]
[Route("[area]/reports/av/overviews")]
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
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindInstance(int id)
    {
        var instance = _overviewInstanceService.FindById(id);
        if (instance == null) return NoContent();

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
    public IActionResult Update([FromBody] AVOverviewInstanceModel model)
    {
        var result = _overviewInstanceService.UpdateAndSave((Entities.AVOverviewInstance)model);
        var instance = _overviewInstanceService.FindById(result.Id) ?? throw new NoContentException("Overview Section does not exist");
        return new JsonResult(new AVOverviewInstanceModel(instance, _serializerOptions));
    }

    /// <summary>
    /// Get all user report instances for the specified instance 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/responses")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<UserAVOverviewInstanceModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult GetUserAVOverviewInstancesAsync(long id)
    {
        var content = _overviewInstanceService.GetUserAVOverviewInstances(id);
        return new JsonResult(content.Select(c => new UserAVOverviewInstanceModel(c)));
    }

    /// <summary>
    /// Add or update an array of user report instances.
    /// These keep track of who a report was sent to and the status.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("response")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserAVOverviewInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult AddOrUpdate([FromBody] UserAVOverviewInstanceModel model)
    {
        var result = _overviewInstanceService.UpdateAndSave((Entities.UserAVOverviewInstance)model);
        return new JsonResult(new UserAVOverviewInstanceModel(result));
    }

    /// <summary>
    /// Add or update an array of user report instances.
    /// These keep track of who a report was sent to and the status.
    /// </summary>
    /// <param name="models"></param>
    /// <returns></returns>
    [HttpPost("responses")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<UserAVOverviewInstanceModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult AddOrUpdate([FromBody] IEnumerable<UserAVOverviewInstanceModel> models)
    {
        var entities = models.Select(m => (Entities.UserAVOverviewInstance)m);
        var result = _overviewInstanceService.UpdateAndSave(entities);
        return new JsonResult(result.Select(r => new UserAVOverviewInstanceModel(r)));
    }

    /// <summary>
    /// Update the report instance status.
    /// </summary>
    /// <param name="id">The report instance id.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="status">The status to update to.</param>
    /// <returns></returns>
    [HttpPut("{id}/{userId}/status/{status}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserAVOverviewInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult UpdateReportLinkStatus(long id, int userId, Entities.ReportStatus status)
    {
        var instance = _overviewInstanceService.GetUserAVOverviewInstance(id, userId) ?? throw new NoContentException();
        instance.Status = status;
        _overviewInstanceService.UpdateAndSave(instance);
        return new JsonResult(new UserAVOverviewInstanceModel(instance));
    }
    #endregion
}
