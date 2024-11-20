using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.ContentReference;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ContentReferenceController class, provides ContentReference endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
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
    private readonly IContentReferenceService _contentReferenceService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentReferenceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentReferenceService"></param>
    public ContentReferenceController(IContentReferenceService contentReferenceService)
    {
        _contentReferenceService = contentReferenceService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content references for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<ContentReferenceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _contentReferenceService.Find(new ContentReferenceFilter(query));
        var page = new Paged<ContentReferenceModel>(result.Items.Select(cr => new ContentReferenceModel(cr)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find content reference for the specified 'id'.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{source}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult FindById(string source, [FromQuery] string uid)
    {
        var result = _contentReferenceService.FindById(new[] { source, uid }) ?? throw new NoContentException();
        return new JsonResult(new ContentReferenceModel(result));
    }

    /// <summary>
    /// Update the content reference for the specified 'source' and 'uid'
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{source}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Update([FromBody] ContentReferenceModel model)
    {
        var result = _contentReferenceService.UpdateAndSave(model.ToEntity());
        return new JsonResult(new ContentReferenceModel(result));
    }

    /// <summary>
    /// Delete the content reference for the specified 'source' and 'uid'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{source}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Delete([FromBody] ContentReferenceModel model)
    {
        _contentReferenceService.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }

    /// <summary>
    /// Find all the content ids for the specified 'uid'.
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("content/ids")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(long[]), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult FindContentId([FromQuery] string uid)
    {
        var result = _contentReferenceService.FindContentIds(uid);
        return new JsonResult(result);
    }
    #endregion
}
