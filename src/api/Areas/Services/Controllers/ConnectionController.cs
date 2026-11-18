using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ConnectionController class, provides Connection endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/connections")]
[Route("api/[area]/connections")]
[Route("v{version:apiVersion}/[area]/connections")]
[Route("[area]/connections")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ConnectionController : ControllerBase
{
    #region Variables
    private readonly IConnectionService _serviceConnection;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ConnectionController object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceConnection"></param>
    /// <param name="serializerOptions"></param>
    public ConnectionController(IConnectionService serviceConnection, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _serviceConnection = serviceConnection;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a connection for the specified 'code'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ConnectionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Connection" })]
    public IActionResult FindById(int id)
    {
        var result = _serviceConnection.FindById(id);
        if (result == null) return NoContent();
        return new JsonResult(new ConnectionModel(result, _serializerOptions));
    }
    #endregion
}
