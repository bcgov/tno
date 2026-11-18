using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Cache;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// CacheController class, provides Cache endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/cache")]
[Route("api/[area]/cache")]
[Route("v{version:apiVersion}/[area]/cache")]
[Route("[area]/cache")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class CacheController : ControllerBase
{
    #region Variables
    private readonly ICacheService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CacheController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public CacheController(ICacheService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of Cache.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<CacheModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Cache" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(m => new CacheModel(m)));
    }
    #endregion
}
