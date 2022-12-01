using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Keycloak;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// KeycloakController class, provides Keycloak endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/keycloak")]
[Route("api/[area]/keycloak")]
[Route("v{version:apiVersion}/[area]/keycloak")]
[Route("[area]/keycloak")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class KeycloakController : ControllerBase
{
    #region Variables
    private readonly IKeycloakHelper _keycloakHelper;
    private readonly IKeycloakService _keycloakService;
    private readonly IUserService _userService;
    private readonly Config.KeycloakOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserController object, initializes with specified parameters.
    /// </summary>
    /// <param name="keycloakHelper"></param>
    /// <param name="keycloakService"></param>
    /// <param name="userService"></param>
    /// <param name="options"></param>
    public KeycloakController(IKeycloakHelper keycloakHelper, IKeycloakService keycloakService, IUserService userService, IOptions<Config.KeycloakOptions> options)
    {
        _keycloakHelper = keycloakHelper;
        _keycloakService = keycloakService;
        _userService = userService;
        _options = options.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Sync users, roles, and claims with Keycloak.
    /// This ensures users, roles, and claims within TNO have their 'Key' linked to Keycloak.
    /// </summary>
    /// <returns></returns>
    [HttpPost("sync")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Keycloak" })]
    public async Task<IActionResult> SyncAsync()
    {
        await _keycloakHelper.SyncAsync();
        return new OkResult();
    }

    /// <summary>
    /// Get an array of roles for the current client.
    /// </summary>
    /// <returns></returns>
    [HttpGet("roles")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string[]), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Keycloak" })]
    public async Task<IActionResult> GetClientRolesAsync()
    {
        if (!_options.ClientId.HasValue) throw new ConfigurationException("Keycloak clientId has not been configured");
        var roles = await _keycloakService.GetRolesAsync(_options.ClientId.Value);
        return new JsonResult(roles?.Select(r => r.Name) ?? Array.Empty<string>());
    }

    /// <summary>
    /// Get an array of roles for the current client.
    /// </summary>
    /// <returns></returns>
    [HttpGet("users/{username}/roles")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string[]), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Keycloak" })]
    public async Task<IActionResult> GetUserRolesAsync(string username)
    {
        if (!_options.ClientId.HasValue) throw new ConfigurationException("Keycloak clientId has not been configured");

        var user = _userService.FindByUsername(username);
        if (user == null) return NoContent();

        var roles = await _keycloakService.GetUserClientRolesAsync(new Guid(user.Key), _options.ClientId.Value);
        return new JsonResult(roles?.Select(r => r.Name) ?? Array.Empty<string>());
    }

    /// <summary>
    /// Update the specified user with the specified roles.
    /// </summary>
    /// <returns></returns>
    [HttpPut("users/{username}/roles")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string[]), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Keycloak" })]
    public async Task<IActionResult> UpdateUserRoles(string username, string[] roles)
    {
        var user = _userService.FindByUsername(username);
        if (user == null) throw new InvalidOperationException("User does not exist");

        var result = await _keycloakHelper.UpdateUserRolesAsync(new Guid(user.Key), roles);
        return new JsonResult(result ?? Array.Empty<string>());
    }
    #endregion
}
