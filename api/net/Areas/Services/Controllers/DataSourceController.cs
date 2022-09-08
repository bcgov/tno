using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// DataSourceController class, provides DataSource endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("services")]
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
    private readonly IDataServiceService _serviceDataService;
    private readonly IDataSourceService _serviceDataSource;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceDataService"></param>
    /// <param name="serviceDataSource"></param>
    /// <param name="serializerOptions"></param>
    public DataSourceController(IDataServiceService serviceDataService, IDataSourceService serviceDataSource, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _serviceDataService = serviceDataService;
        _serviceDataSource = serviceDataSource;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a data source for the specified 'code'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{code}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DataSourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult FindByCode(string code)
    {
        var result = _serviceDataSource.FindByCode(code);
        if (result == null) return new NoContentResult();
        return new JsonResult(new DataSourceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Find an array of data sources for the specified media type.
    /// </summary>
    /// <returns></returns>
    [HttpGet("for/media/type/{mediaTypeName}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<DataSourceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult FindByMediaType(string mediaTypeName)
    {
        var result = _serviceDataSource.FindByMediaType(mediaTypeName);
        return new JsonResult(result.Select(ds => new DataSourceModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Get an array of data sources.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<DataSourceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult GetDataSources()
    {
        var result = _serviceDataSource.FindAll(true);
        return new JsonResult(result.Select(ds => new DataSourceModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Update data-service associated with the specified data source in database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DataSourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "DataSource" })]
    public IActionResult Update(DataSourceModel model)
    {
        _serviceDataService.AddOrUpdate(model.ToEntity(_serializerOptions).DataService!);

        var result = _serviceDataSource.FindById(model.Id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new DataSourceModel(result, _serializerOptions));
    }
    #endregion
}
