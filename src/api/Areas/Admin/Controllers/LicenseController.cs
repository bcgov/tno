using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.License;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// LicenseController class, provides License endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/licences")]
[Route("api/[area]/licences")]
[Route("v{version:apiVersion}/[area]/licences")]
[Route("[area]/licences")]
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
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<LicenseModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "License" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new LicenseModel(ds)));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LicenseModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "License" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new LicenseModel(result));
    }

    /// <summary>
    /// Add content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LicenseModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "License" })]
    public IActionResult Add([FromBody] LicenseModel model)
    {
        var result = _service.AddAndSave(model.ToEntity());
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new LicenseModel(result));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LicenseModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "License" })]
    public IActionResult Update([FromBody] LicenseModel model)
    {
        var result = _service.UpdateAndSave(model.ToEntity());
        return new JsonResult(new LicenseModel(result));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(LicenseModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "License" })]
    public IActionResult Delete([FromBody] LicenseModel model)
    {
        _service.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }
    #endregion
}
