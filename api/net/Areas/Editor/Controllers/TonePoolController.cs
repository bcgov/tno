using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.TonePool;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// TonePoolController class, provides TonePool endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/tone/pools")]
[Route("api/[area]/tone/pools")]
[Route("v{version:apiVersion}/[area]/tone/pools")]
[Route("[area]/tone/pools")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class TonePoolController : ControllerBase
{
    #region Variables
    private readonly ITonePoolService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TonePoolController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public TonePoolController(ITonePoolService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of TonePool.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<TonePoolModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "TonePool" })]
    [ETagCacheTableFilter("tone_pools")]
    [ResponseCache(Duration = 5 * 60)]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new TonePoolModel(c)));
    }
    #endregion
}
