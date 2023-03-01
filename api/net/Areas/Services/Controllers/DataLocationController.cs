using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.DataLocation;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// DataLocationController class, provides DataLocation endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/data/locations")]
[Route("api/[area]/data/locations")]
[Route("v{version:apiVersion}/[area]/data/locations")]
[Route("[area]/data/locations")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class DataLocationController : ControllerBase
{
    #region Variables
    private readonly IDataLocationService _serviceDataLocation;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataLocationController object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceDataLocation"></param>
    /// <param name="serializerOptions"></param>
    public DataLocationController(IDataLocationService serviceDataLocation, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _serviceDataLocation = serviceDataLocation;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a data location for the specified 'id'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DataLocationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "DataLocation" })]
    public IActionResult FindById(int id)
    {
        var result = _serviceDataLocation.FindById(id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new DataLocationModel(result, _serializerOptions));
    }

    /// <summary>
    /// Find a data location for the specified 'name'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{name}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DataLocationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "DataLocation" })]
    public IActionResult FindByName(string name)
    {
        var result = _serviceDataLocation.FindByName(name);
        if (result == null) return new NoContentResult();
        return new JsonResult(new DataLocationModel(result, _serializerOptions));
    }
    #endregion
}
