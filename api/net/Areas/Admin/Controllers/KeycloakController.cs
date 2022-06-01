using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Keycloak;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// KeycloakController class, provides Keycloak endpoints for the admin api.
/// </summary>
[Authorize]
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserController object, initializes with specified parameters.
    /// </summary>
    /// <param name="keycloakHelper"></param>
    public KeycloakController(IKeycloakHelper keycloakHelper)
    {
        _keycloakHelper = keycloakHelper;
    }
    #endregion

    #region Endpoints

    /// <summary>
    /// Sync users, roles, and claims with Keycloak.
    /// This ensures users, roles, and claims within TNO have their 'Key' linked to Keycloak.
    /// </summary>
    /// <returns></returns>
    [HttpPost("sync")]
    [Produces("application/json")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Keycloak" })]
    public async Task<IActionResult> SyncAsync()
    {
        await _keycloakHelper.SyncAsync();
        return new OkResult();
    }
    #endregion
}
