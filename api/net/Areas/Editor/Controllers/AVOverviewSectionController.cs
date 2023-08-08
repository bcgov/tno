using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.AvOverview;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// AVOverviewSectionController class, provides AVOverviewSection endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/overview/sections")]
[Route("api/[area]/overview/sections")]
[Route("v{version:apiVersion}/[area]/overview/sections")]
[Route("[area]/overview/sections")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class AVOverviewSectionController : ControllerBase
{
    #region Variables
    private readonly IAVOverviewSectionService _overviewSectionService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewSectionController object, initializes with specified parameters.
    /// </summary>
    /// <param name="overviewSectionService"></param>
    /// <param name="serializerOptions"></param>
    public AVOverviewSectionController(
        IAVOverviewSectionService overviewSectionService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _overviewSectionService = overviewSectionService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all AVOverviewSections.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AVOverviewSectionModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_overviewSectionService.FindAll().Select(ds => new AVOverviewSectionModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find AVOverviewSection for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewSectionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "AvOverviewSection" })]
    public IActionResult FindById(int id)
    {
        var result = _overviewSectionService.FindById(id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new AVOverviewSectionModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add overview section for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewSectionModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Add(AVOverviewSectionModel model)
    {
        var result = _overviewSectionService.AddAndSave(model.ToEntity(_serializerOptions));
        var overviewSection = _overviewSectionService.FindById(result.Id) ?? throw new InvalidOperationException("Overview Section does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new AVOverviewSectionModel(overviewSection, _serializerOptions));
    }

    /// <summary>
    /// Update overview section for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewSectionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Update(AVOverviewSectionModel model)
    {
        var result = _overviewSectionService.UpdateAndSave(model.ToEntity(_serializerOptions));
        var overviewSection = _overviewSectionService.FindById(result.Id) ?? throw new InvalidOperationException("Overview Section does not exist");
        return new JsonResult(new AVOverviewSectionModel(overviewSection, _serializerOptions));
    }

    /// <summary>
    /// Delete overview section for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewSectionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Delete(AVOverviewSectionModel model)
    {
        _overviewSectionService.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }
    #endregion
}
