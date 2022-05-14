using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.ContentReference;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Entities.Models;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ContentReferenceController class, provides ContentReference endpoints for the api.
/// </summary>
[Authorize]
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
    /// Find a page of content references for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<ContentReferenceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new DAL.Models.ContentReferenceFilter(query));
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
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult FindById(string source, [FromQuery] string uid)
    {
        var result = _service.FindById(new[] { source, uid });

        if (result == null) return new NoContentResult();
        return new JsonResult(new ContentReferenceModel(result));
    }

    /// <summary>
    /// Update the content reference for the specified 'source' and 'uid'
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{source}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Update(ContentReferenceModel model)
    {
        var result = _service.Update(model.ToEntity());
        return new JsonResult(new ContentReferenceModel(result));
    }

    /// <summary>
    /// Delete the content reference for the specified 'source' and 'uid'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{source}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Delete(ContentReferenceModel model)
    {
        _service.Delete(model.ToEntity());
        return new JsonResult(model);
    }
    #endregion
}
