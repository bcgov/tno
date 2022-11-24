using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.Core.Exceptions;
using TNO.CSS.API.Config;
using TNO.CSS.Models;
using TNO.Keycloak;

namespace TNO.CSS.API.Controllers;

/// <summary>
///
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/integrations/{integrationId}/{environment}/[controller]")]
public class RolesController : ControllerBase
{
    #region Variables
    private readonly IKeycloakService _service;
    private readonly CssOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="options"></param>
    public RolesController(IKeycloakService service, IOptions<CssOptions> options)
    {
        _service = service;
        _options = options.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DataResponseModel<RoleModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.UnprocessableEntity)]
    [SwaggerOperation(Tags = new[] { "Roles" })]
    public async Task<IActionResult> GetAsync()
    {
        var clientId = _options.ClientId ?? throw new ConfigurationException("Keycloak 'ClientId' is not configured.");

        var roles = await _service.GetRolesAsync(clientId);
        var response = new DataResponseModel<RoleModel>()
        {
            Data = roles.Select(r => new RoleModel(r.Name ?? "", r.Composite))
        };
        return new JsonResult(response);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet("{name}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RoleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.UnprocessableEntity)]
    [SwaggerOperation(Tags = new[] { "Roles" })]
    public async Task<IActionResult> FindByNameAsync(string name)
    {
        var clientId = _options.ClientId ?? throw new ConfigurationException("Keycloak 'ClientId' is not configured.");

        var role = await _service.GetRoleAsync(clientId, name);
        if (role == null) return NotFound(new ErrorResponseModel());

        return new JsonResult(new RoleModel(role.Name ?? "", role.Composite));
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RoleModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.UnprocessableEntity)]
    [SwaggerOperation(Tags = new[] { "Roles" })]
    public async Task<IActionResult> AddAsync(RoleModel model, ApiVersion version)
    {
        var clientId = _options.ClientId ?? throw new ConfigurationException("Keycloak 'ClientId' is not configured.");

        var role = new TNO.Keycloak.Models.RoleModel()
        {
            Name = model.Name,
            ClientRole = true,
            Composite = model.Composite
        };
        await _service.CreateRoleAsync(clientId, role);
        return CreatedAtAction(nameof(FindByNameAsync), new
        {
            name = model.Name,
            version = version.ToString(),
            integrationId = this.RouteData.Values["integrationId"],
            environment = this.RouteData.Values["environment"]
        }, model);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpPut("{name}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RoleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.UnprocessableEntity)]
    [SwaggerOperation(Tags = new[] { "Roles" })]
    public async Task<IActionResult> Update(string name, RoleModel model)
    {
        var clientId = _options.ClientId ?? throw new ConfigurationException("Keycloak 'ClientId' is not configured.");

        var role = await _service.GetRoleAsync(clientId, name);
        if (role == null) return NotFound(new ErrorResponseModel());

        role.Name = model.Name;
        role.Composite = model.Composite;
        await _service.UpdateRoleAsync(clientId, role);
        return new JsonResult(model);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{name}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RoleModel), (int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.UnprocessableEntity)]
    [SwaggerOperation(Tags = new[] { "Roles" })]
    public async Task<IActionResult> Delete(string name)
    {
        var clientId = _options.ClientId ?? throw new ConfigurationException("Keycloak 'ClientId' is not configured.");

        var role = await _service.GetRoleAsync(clientId, name);
        if (role == null) return NotFound(new ErrorResponseModel());

        await _service.DeleteRoleAsync(clientId, name);
        return NoContent();
    }
    #endregion
}
