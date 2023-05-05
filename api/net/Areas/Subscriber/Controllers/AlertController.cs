using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Alert;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// AlertController class, provides alert endpoints for the admin api.
/// </summary>
[ApiController]
[AllowAnonymous]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/alerts")]
[Route("api/[area]/alerts")]
[Route("v{version:apiVersion}/[area]/alerts")]
[Route("[area]/alerts")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class AlertController : ControllerBase
{
    #region Variables
    private readonly IAlertService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AlertController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public AlertController(IAlertService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of alert for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AlertModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Alert" })]
    public IActionResult FindAlert()
    {
         var result = _service.FindAlert();
        if (result == null) return new NoContentResult();
        return new JsonResult(new AlertModel(result));
    }
    #endregion
}
