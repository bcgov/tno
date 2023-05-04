using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Alert;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// AlertController class, provides alert endpoints for the admin api.
/// </summary>
[ApiController]
[Area("admin")]
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

    /// <summary>
    /// Add alert for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ClientRoleAuthorize(ClientRole.Administrator)]
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AlertModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Alert" })]
    public IActionResult Add(AlertModel model)
    {
        var result = _service.AddAndSave((Alert)model);
        return CreatedAtAction(nameof(FindAlert), new { id = result.Id }, new AlertModel(result));
    }

    /// <summary>
    /// Update alert for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ClientRoleAuthorize(ClientRole.Administrator)]
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AlertModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Alert" })]
    public IActionResult Update(AlertModel model)
    {
        var result = _service.UpdateAndSave((Alert)model);
        return new JsonResult(new AlertModel(result));
    }

    /// <summary>
    /// Delete alert for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ClientRoleAuthorize(ClientRole.Administrator)]
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AlertModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Alert" })]
    public IActionResult Delete(AlertModel model)
    {
        _service.DeleteAndSave((Alert)model);
        return new JsonResult(model);
    }
    #endregion
}
