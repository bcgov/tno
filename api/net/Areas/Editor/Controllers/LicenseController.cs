using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.License;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// LicenseController class, provides License endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/licenses")]
[Route("api/[area]/licenses")]
[Route("v{version:apiVersion}/[area]/licenses")]
[Route("[area]/licenses")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class LicenseController : ControllerBase
{
    #region Variables
    private readonly ILicenseService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LicenseController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public LicenseController(ILicenseService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of License.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<LicenseModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "License" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new LicenseModel(c)));
    }
    #endregion
}
