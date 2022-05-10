using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.MediaType;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// MediaTypeController class, provides MediaType endpoints for the admin api.
/// </summary>
[Authorize]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/media/types")]
[Route("api/[area]/media/types")]
[Route("v{version:apiVersion}/[area]/media/types")]
[Route("[area]/media/types")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class MediaTypeController : ControllerBase
{
    #region Variables
    private readonly IMediaTypeService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MediaTypeController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public MediaTypeController(IMediaTypeService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of media type for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<MediaTypeModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "MediaType" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new MediaTypeModel(ds)));
    }

    /// <summary>
    /// Find a page of media type for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<MediaTypeModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "MediaType" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new MediaTypeFilter(query));
        var page = new Paged<MediaTypeModel>(result.Items.Select(ds => new MediaTypeModel(ds)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find media type for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MediaTypeModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "MediaType" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new MediaTypeModel(result));
    }

    /// <summary>
    /// Add media type for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MediaTypeModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "MediaType" })]
    public IActionResult Add(MediaTypeModel model)
    {
        var result = _service.Add((MediaType)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new MediaTypeModel(result));
    }

    /// <summary>
    /// Update media type for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MediaTypeModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "MediaType" })]
    public IActionResult Update(MediaTypeModel model)
    {
        var result = _service.Update((MediaType)model);
        return new JsonResult(new MediaTypeModel(result));
    }

    /// <summary>
    /// Delete media type for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MediaTypeModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "MediaType" })]
    public IActionResult Delete(MediaTypeModel model)
    {
        _service.Delete((MediaType)model);
        return new JsonResult(model);
    }
    #endregion
}
