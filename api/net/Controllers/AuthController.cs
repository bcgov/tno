using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Keycloak;
using TNO.API.Models.Auth;

namespace TNO.API.Controllers;

/// <summary>
/// AuthController class, provides health endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    #region Variables
    private readonly IKeycloakHelper _keycloakHelper;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AuthController object, initializes with specified parameters.
    /// </summary>
    /// <param name="keycloakHelper"></param>
    public AuthController(IKeycloakHelper keycloakHelper)
    {
        _keycloakHelper = keycloakHelper;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Get user information.
    /// If the user doesn't currently exist in TNO, activate a new user by adding them to TNO.
    /// If the user exists in TNO, activate user by linking to Keycloak and updating Keycloak.
    /// </summary>
    /// <returns></returns>
    [HttpPost("userinfo")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PrincipalModel), 200)]
    [SwaggerOperation(Tags = new[] { "health" })]
    public async Task<IActionResult> UserInfoAsync()
    {
        var user = await _keycloakHelper.ActivateAsync(this.User);
        return new JsonResult(new PrincipalModel(this.User, user));
    }
    #endregion
}
