using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Role;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// RoleController class, provides Role endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/roles")]
[Route("api/[area]/roles")]
[Route("v{version:apiVersion}/[area]/roles")]
[Route("[area]/roles")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class RoleController : ControllerBase
{
    #region Variables
    private readonly IKeycloakService _service;
    private readonly Config.KeycloakOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a RoleController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="options"></param>
    public RoleController(IKeycloakService service, IOptions<Config.KeycloakOptions> options)
    {
        _service = service;
        _options = options.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of Role.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<RoleModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "Role" })]
    [ETagCacheTableFilter("roles")]
    [ResponseCache(Duration = 5 * 60)]
    public async Task<IActionResult> FindAllAsync()
    {
        if (!_options.ClientId.HasValue) throw new ConfigurationException("Keycloak clientId has not been configured");

        var roles = await _service.GetRolesAsync(_options.ClientId.Value);
        return new JsonResult(roles.Select(c => new RoleModel(c.Name!)));
    }
    #endregion
}
