using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Claim;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// ClaimController class, provides Claim endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/claims")]
[Route("api/[area]/claims")]
[Route("v{version:apiVersion}/[area]/claims")]
[Route("[area]/claims")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ClaimController : ControllerBase
{
    #region Variables
    private readonly IClaimService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ClaimController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public ClaimController(IClaimService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of Claim.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<ClaimModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Claim" })]
    public IActionResult Find()
    {
        return new JsonResult(_service.FindAll().Select(c => new ClaimModel(c)));
    }
    #endregion
}
