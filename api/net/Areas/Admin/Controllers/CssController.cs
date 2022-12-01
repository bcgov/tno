using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.CSS;
using TNO.API.Models;
using TNO.CSS;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// CssController class, provides CSS endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/css")]
[Route("api/[area]/css")]
[Route("v{version:apiVersion}/[area]/css")]
[Route("[area]/css")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class CssController : ControllerBase
{
    #region Variables
    private readonly ICssHelper _cssHelper;
    private readonly ICssEnvironmentService _cssService;
    private readonly IUserService _userService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserController object, initializes with specified parameters.
    /// </summary>
    /// <param name="cssHelper"></param>
    /// <param name="cssService"></param>
    /// <param name="userService"></param>
    public CssController(ICssHelper cssHelper, ICssEnvironmentService cssService, IUserService userService)
    {
        _cssHelper = cssHelper;
        _cssService = cssService;
        _userService = userService;
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
        await _cssHelper.SyncAsync();
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
        var roles = await _cssService.GetRolesAsync();
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
        var user = _userService.FindByUsername(username);
        if (user == null) return NoContent();

        var userRoles = await _cssService.GetRolesForUserAsync(user.Key);
        return new JsonResult(userRoles.Roles?.Select(r => r.Name) ?? Array.Empty<string>());
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

        var result = await _cssHelper.UpdateUserRolesAsync(user.Key, roles);
        return new JsonResult(result ?? Array.Empty<string>());
    }
    #endregion
}
