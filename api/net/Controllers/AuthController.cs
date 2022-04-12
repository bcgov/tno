using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models.Auth;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Entities;

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
    private readonly IUserService _userService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AuthController object, initializes with specified parameters.
    /// </summary>
    /// <param name="userService"></param>
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return environment information.
    /// </summary>
    /// <returns></returns>
    [HttpPost("userinfo")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PrincipalModel), 200)]
    [SwaggerOperation(Tags = new[] { "health" })]
    public IActionResult Auth()
    {
        var key = this.User.GetUid();
        var user = _userService.FindByKey(key);

        // TODO: Add custom attribute to Keycloak to store the user.Id.  Then remove need to fetch from database.
        // If user doesn't exist, add them to the database.
        if (user == null)
        {
            var username = this.User.GetUsername() ?? throw new InvalidOperationException("Username is required");
            var email = this.User.GetEmail() ?? throw new InvalidOperationException("Email is required");
            user = _userService.Add(new User(username, email, key)
            {
                DisplayName = this.User.GetDisplayName() ?? username,
                FirstName = this.User.GetFirstName() ?? "",
                LastName = this.User.GetLastName() ?? "",
                IsEnabled = true,
                EmailVerified = true,
                IsSystemAccount = false,
                LastLoginOn = DateTime.UtcNow,
            });
        }

        return new JsonResult(new PrincipalModel(this.User, user));
    }
    #endregion
}
