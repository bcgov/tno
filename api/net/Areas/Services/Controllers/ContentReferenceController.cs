using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ContentReferenceController class, provides ContentReference endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/content/references")]
[Route("api/[area]/content/references")]
[Route("v{version:apiVersion}/[area]/content/references")]
[Route("[area]/content/references")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ContentReferenceController : ControllerBase
{
    #region Variables
    private readonly IContentReferenceService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentReferenceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public ContentReferenceController(IContentReferenceService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{source}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult FindByKey(string source, [FromQuery] string uid)
    {
        var result = _service.FindByKey(source, uid);
        if (result == null) return new NoContentResult();
        return new JsonResult(new ContentReferenceModel(result));
    }

    /// <summary>
    /// Add new content reference to database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Add(ContentReferenceModel model)
    {
        var result = _service.AddAndSave(model.ToEntity());
        return CreatedAtAction(nameof(FindByKey), new { source = result.Source, uid = result.Uid }, new ContentReferenceModel(result));
    }

    /// <summary>
    /// Update content reference in database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{source}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Update(ContentReferenceModel model)
    {
        var result = _service.UpdateAndSave(model.ToEntity());
        return new JsonResult(new ContentReferenceModel(result));
    }
    #endregion
}
