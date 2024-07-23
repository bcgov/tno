using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.User;
using TNO.API.Keycloak;
using TNO.API.Models;
using TNO.API.Models.Auth;
using TNO.Core.Exceptions;
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
    private readonly IKeycloakHelper _keycloakHelper;
    private readonly IUserService _userService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AuthController object, initializes with specified parameters.
    /// </summary>
    /// <param name="keycloakHelper"></param>
    /// <param name="userService"></param>
    public AuthController(IKeycloakHelper keycloakHelper, IUserService userService)
    {
        _keycloakHelper = keycloakHelper;
        _userService = userService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Get user information.
    /// If the user doesn't currently exist in TNO, activate a new user by adding them to TNO.
    /// If the user exists in TNO, activate user by linking to Keycloak and updating Keycloak.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPost("userinfo")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PrincipalModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Auth" })]
    public async Task<IActionResult> UserInfoAsync(LocationModel? location)
    {
        var state = await _keycloakHelper.ActivateAsync(this.User, location);
        return new JsonResult(new PrincipalModel(this.User, state.Item1, state.Item2));
    }

    /// <summary>
    /// Get user information.
    /// If the user doesn't currently exist in TNO, activate a new user by adding them to TNO.
    /// If the user exists in TNO, activate user by linking to Keycloak and updating Keycloak.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPost("logout")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PrincipalModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Auth" })]
    public IActionResult Logout(LocationModel? location)
    {
        _keycloakHelper.RemoveLocation(this.User, location?.Key);
        return Ok();
    }

    /// <summary>
    /// Request a code to validate a preapproved email address.
    /// If a code is provided it will validate the code.
    /// If the code is valid it will apply the roles to the user's account and remove the duplicate preapproved account.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("request/code")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RegisterModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Auth" })]
    public async Task<IActionResult> RequestCodeAsync(RegisterModel model)
    {
        // Get the account with the preapproved email address.
        // If multiple accounts have the same email address we cannot preapprove.
        var users = _userService.FindByEmail(model.Email);
        if (users.Count() != 1) return new JsonResult(new RegisterModel(model.Email, "Your account is not preapproved."));

        var preapproved = users.First();
        if (preapproved.Status != UserStatus.Preapproved) return new JsonResult(new RegisterModel(model.Email, "Your account is not preapproved"));

        // Get the current authenticated user account.
        var username = this.User.GetUsername() ?? throw new NotAuthorizedException();
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException();

        if (!String.IsNullOrWhiteSpace(model.Code))
        {
            var expires = user.CodeCreatedOn?.AddMinutes(30);
            if (user.Code != model.Code && expires != null && expires < DateTime.UtcNow) throw new InvalidOperationException("Code has expired or is not valid");

            user.Code = "";
            user.Status = UserStatus.Approved;
            user.Roles = preapproved.Roles;
            var key = Guid.Parse(user.Key);
            await _keycloakHelper.UpdateUserRolesAsync(key, preapproved.Roles.Split(",").Select(r => r[1..^1]).ToArray());
            _userService.UpdateAndSave(user);
            _userService.DeleteAndSave(preapproved);
            return new JsonResult(new RegisterModel(model.Email, user.Status, $"An email has been sent to {model.Email}"));
        }
        else
        {
            // TODO: Send email.
            var rnd = new Random();
            user.Code = $"{rnd.Next()}";
            _userService.UpdateAndSave(user);

            return new JsonResult(new RegisterModel(model.Email, UserStatus.Approved, "Your account has been approved"));
        }
    }

    /// <summary>
    /// Update user for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("request/approval")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Auth" })]
    public IActionResult RequestApproval(UserModel model)
    {
        // Only allow a user to update their own account.
        var username = this.User.GetUsername() ?? throw new NotAuthorizedException("Cannot update user");
        var original = _userService.FindByUsername(username) ?? throw new InvalidOperationException("Cannot update user");
        original.Note = model.Note;
        original.Status = Entities.UserStatus.Requested;
        var result = _userService.UpdateAndSave(original);
        return new JsonResult(new UserModel(result));
    }
    #endregion
}
