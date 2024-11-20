using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Contributor;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ContributorController class, provides contributor endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/contributors")]
[Route("api/[area]/contributors")]
[Route("v{version:apiVersion}/[area]/contributors")]
[Route("[area]/contributors")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ContributorController : ControllerBase
{
    #region Variables
    private readonly IContributorService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContributorController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public ContributorController(IContributorService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of contributor for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ContributorModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Contributor" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new ContributorModel(ds)));
    }

    /// <summary>
    /// Find contributor for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContributorModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Contributor" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new ContributorModel(result));
    }

    /// <summary>
    /// Add a contributor.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContributorModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Contributor" })]
    public IActionResult Add([FromBody] ContributorModel model)
    {
        var result = _service.AddAndSave((Contributor)model);
        result = _service.FindById(result.Id) ?? throw new NoContentException();
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ContributorModel(result));
    }

    /// <summary>
    /// Update contributor for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContributorModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Contributor" })]
    public IActionResult Update([FromBody] ContributorModel model)
    {
        var result = _service.UpdateAndSave((Contributor)model);
        result = _service.FindById(result.Id) ?? throw new NoContentException();
        return new JsonResult(new ContributorModel(result));
    }

    /// <summary>
    /// Delete contributor for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContributorModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Contributor" })]
    public IActionResult Delete([FromBody] ContributorModel model)
    {
        _service.DeleteAndSave((Contributor)model);
        return new JsonResult(model);
    }
    #endregion
}
