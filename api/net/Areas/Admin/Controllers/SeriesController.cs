using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Series;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// SeriesController class, provides series endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/series")]
[Route("api/[area]/series")]
[Route("v{version:apiVersion}/[area]/series")]
[Route("[area]/series")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class SeriesController : ControllerBase
{
    #region Variables
    private readonly ISeriesService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SeriesController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public SeriesController(ISeriesService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of series for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<SeriesModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Series" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new SeriesModel(ds)));
    }

    /// <summary>
    /// Find a page of series for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<SeriesModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Series" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new SeriesFilter(query));
        var page = new Paged<SeriesModel>(result.Items.Select(ds => new SeriesModel(ds)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find series for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(SeriesModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Series" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new SeriesModel(result));
    }

    /// <summary>
    /// Add a series.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(SeriesModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Series" })]
    public IActionResult Add(SeriesModel model)
    {
        var result = _service.AddAndSave((Series)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new SeriesModel(result));
    }

    /// <summary>
    /// Update series for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(SeriesModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Series" })]
    public IActionResult Update(SeriesModel model)
    {
        var result = _service.UpdateAndSave((Series)model);
        return new JsonResult(new SeriesModel(result));
    }

    /// <summary>
    /// Delete series for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(SeriesModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Series" })]
    public IActionResult Delete(SeriesModel model)
    {
        _service.DeleteAndSave((Series)model);
        return new JsonResult(model);
    }
    #endregion
}
