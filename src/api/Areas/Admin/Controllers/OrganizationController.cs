using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Organization;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// OrganizationController class, provides organization endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/organizations")]
[Route("api/[area]/organizations")]
[Route("v{version:apiVersion}/[area]/organizations")]
[Route("[area]/organizations")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class OrganizationController : ControllerBase
{
    #region Variables
    private readonly IOrganizationService _organizationService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a OrganizationController object, initializes with specified parameters.
    /// </summary>
    /// <param name="organizationService"></param>
    public OrganizationController(
        IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all organizations.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<OrganizationModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Organization" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_organizationService.FindAll().Select(ds => new OrganizationModel(ds)));
    }

    /// <summary>
    /// Find organization for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(OrganizationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Organization" })]
    public IActionResult FindById(int id)
    {
        var result = _organizationService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new OrganizationModel(result));
    }

    /// <summary>
    /// Add organization for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(OrganizationModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Organization" })]
    public IActionResult Add([FromBody] OrganizationModel model)
    {
        var result = _organizationService.AddAndSave((Entities.Organization)model);
        var organization = _organizationService.FindById(result.Id) ?? throw new NoContentException("Organization does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new OrganizationModel(organization));
    }

    /// <summary>
    /// Update organization for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(OrganizationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Organization" })]
    public IActionResult Update([FromBody] OrganizationModel model)
    {
        var result = _organizationService.UpdateAndSave((Entities.Organization)model);
        var organization = _organizationService.FindById(result.Id) ?? throw new NoContentException("Organization does not exist");
        return new JsonResult(new OrganizationModel(organization));
    }

    /// <summary>
    /// Delete organization for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(OrganizationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Organization" })]
    public IActionResult Delete([FromBody] OrganizationModel model)
    {
        _organizationService.DeleteAndSave((Entities.Organization)model);
        return new JsonResult(model);
    }
    #endregion
}
