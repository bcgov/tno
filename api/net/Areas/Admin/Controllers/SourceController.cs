using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Source;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// SourceController class, provides Source endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/sources")]
[Route("api/[area]/sources")]
[Route("v{version:apiVersion}/[area]/sources")]
[Route("[area]/sources")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class SourceController : ControllerBase
{
    #region Variables
    private readonly ISourceService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SourceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public SourceController(ISourceService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<SourceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Source" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new SourceModel(ds)));
    }

    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("find")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<SourceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Source" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new SourceFilter(query));
        var page = new Paged<SourceModel>(result.Items.Select(ds => new SourceModel(ds)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Source" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new SourceModel(result));
    }

    /// <summary>
    /// Add content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SourceModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Source" })]
    public IActionResult Add(SourceModel model)
    {
        var result = _service.AddAndSave(model.ToEntity());
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new SourceModel(result));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Source" })]
    public IActionResult Update(SourceModel model)
    {
        var result = _service.UpdateAndSave(model.ToEntity(), true);
        return new JsonResult(new SourceModel(result));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Source" })]
    public IActionResult Delete(SourceModel model)
    {
        _service.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }
    #endregion
}
