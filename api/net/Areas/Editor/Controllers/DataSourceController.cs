using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.DataSource;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Entities.Models;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// DataSourceController class, provides DataSource endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
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
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        return new JsonResult(_service.FindAll().Select(ds => new DataSourceModel(ds, _serializerOptions)));
    }
    #endregion
}
