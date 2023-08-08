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

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// AVOverviewSectionItemController class, provides AVOverviewSectionItem endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/overview/section/items")]
[Route("api/[area]/overview/section/items")]
[Route("v{version:apiVersion}/[area]/overview/section/items")]
[Route("[area]/overview/section/items")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class AVOverviewSectionItemController : ControllerBase
{
    #region Variables
    private readonly IAVOverviewSectionItemService _overviewSectionItemService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewSectionItemController object, initializes with specified parameters.
    /// </summary>
    /// <param name="overviewSectionItemService"></param>
    /// <param name="serializerOptions"></param>
    public AVOverviewSectionItemController(
        IAVOverviewSectionItemService overviewSectionItemService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _overviewSectionItemService = overviewSectionItemService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all AVOverviewSectionItems.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AVOverviewSectionItemModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_overviewSectionItemService.FindAll().Select(ds => new AVOverviewSectionItemModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find AVOverviewSectionItem for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewSectionItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindById(int id)
    {
        var result = _overviewSectionItemService.FindById(id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new AVOverviewSectionItemModel(result, _serializerOptions));
    }

    /// <summary>
    /// Find AVOverviewSectionItem for the specified section 'id'.
    /// </summary>
    /// <param name="sectionId"></param>
    /// <returns></returns>
    [HttpGet("for/section/{sectionId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AVOverviewSectionItemModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindBySectionId(int sectionId)
    {
        return new JsonResult(_overviewSectionItemService.FindBySectionId(sectionId).Select(ds => new AVOverviewSectionItemModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Add overview section for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewSectionItemModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Add(AVOverviewSectionItemModel model)
    {
        var result = _overviewSectionItemService.AddAndSave(model.ToEntity(_serializerOptions));
        var overviewSection = _overviewSectionItemService.FindById(result.Id) ?? throw new InvalidOperationException("Overview Section does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new AVOverviewSectionItemModel(overviewSection, _serializerOptions));
    }

    /// <summary>
    /// Update overview section for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewSectionItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Update(AVOverviewSectionItemModel model)
    {
        var result = _overviewSectionItemService.UpdateAndSave(model.ToEntity(_serializerOptions));
        var overviewSection = _overviewSectionItemService.FindById(result.Id) ?? throw new InvalidOperationException("Overview Section does not exist");
        return new JsonResult(new AVOverviewSectionItemModel(overviewSection, _serializerOptions));
    }

    /// <summary>
    /// Delete overview section for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewSectionItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Delete(AVOverviewSectionItemModel model)
    {
        _overviewSectionItemService.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }
    #endregion
}
