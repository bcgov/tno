using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.DataSource;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities.Models;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// DataSourceController class, provides DataSource endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/data/sources")]
[Route("api/[area]/data/sources")]
[Route("v{version:apiVersion}/[area]/data/sources")]
[Route("[area]/data/sources")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class DataSourceController : ControllerBase
{
    #region Variables
    private readonly IDataSourceService _service;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serializerOptions"></param>
    public DataSourceController(IDataSourceService service, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<DataSourceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new DataSourceModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("find")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<DataSourceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new DataSourceFilter(query));
        var page = new Paged<DataSourceModel>(result.Items.Select(ds => new DataSourceModel(ds, _serializerOptions)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DataSourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new DataSourceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DataSourceModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult Add(DataSourceModel model)
    {
        var result = _service.Add(model.ToEntity(_serializerOptions));
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new DataSourceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DataSourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult Update(DataSourceModel model)
    {
        var result = _service.Update(model.ToEntity(_serializerOptions), true);
        return new JsonResult(new DataSourceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DataSourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult Delete(DataSourceModel model)
    {
        _service.Delete(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }
    #endregion
}
