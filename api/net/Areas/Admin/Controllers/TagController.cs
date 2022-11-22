using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Tag;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// TagController class, provides category endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/tags")]
[Route("api/[area]/tags")]
[Route("v{version:apiVersion}/[area]/tags")]
[Route("[area]/tags")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class TagController : ControllerBase
{
    #region Variables
    private readonly ITagService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TagController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public TagController(ITagService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of category for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<TagModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Tag" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new TagModel(ds)));
    }

    /// <summary>
    /// Find a page of category for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<TagModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Tag" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new TagFilter(query));
        var page = new Paged<TagModel>(result.Items.Select(ds => new TagModel(ds)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find category for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TagModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Tag" })]
    public IActionResult FindById(string id)
    {
        var result = _service.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new TagModel(result));
    }

    /// <summary>
    /// Add category for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TagModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Tag" })]
    public IActionResult Add(TagModel model)
    {
        var result = _service.AddAndSave((Tag)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new TagModel(result));
    }

    /// <summary>
    /// Update category for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TagModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Tag" })]
    public IActionResult Update(TagModel model)
    {
        var result = _service.UpdateAndSave((Tag)model);
        return new JsonResult(new TagModel(result));
    }

    /// <summary>
    /// Delete category for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TagModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Tag" })]
    public IActionResult Delete(TagModel model)
    {
        _service.DeleteAndSave((Tag)model);
        return new JsonResult(model);
    }
    #endregion
}
