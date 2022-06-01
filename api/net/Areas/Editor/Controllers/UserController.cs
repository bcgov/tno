using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.User;
using TNO.API.Filters;
using TNO.API.Keycloak;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// UserController class, provides User endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/users")]
[Route("api/[area]/users")]
[Route("v{version:apiVersion}/[area]/users")]
[Route("[area]/users")]
public class UserController : ControllerBase
{
    #region Variables
    private readonly IUserService _userService;
    private readonly IKeycloakHelper _keycloakHelper;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserController object, initializes with specified parameters.
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="keycloakHelper"></param>
    public UserController(IUserService userService, IKeycloakHelper keycloakHelper)
    {
        _userService = userService;
        _keycloakHelper = keycloakHelper;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of User.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<UserModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "User" })]
    [ETagCacheTableFilter("users")]
    [ResponseCache(Duration = 5 * 60)]
    public IActionResult FindAll()
    {
        var users = _userService.FindAll()
            .Where(u => !u.IsSystemAccount)
            .Select(u => new UserModel(u));
        return new JsonResult(users);
    }

    /// <summary>
    /// Find user for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "User" })]
    public IActionResult FindById(int id)
    {
        var result = _userService.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new Admin.Models.User.UserModel(result));
    }

    /// <summary>
    /// Request a code to validate a preapproved email address.
    /// If a code is provied it will validate the code.
    /// If the code is valid it will apply the roles to the user's account and remove the duplicate preapproved account.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("request/code")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RegisterModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "User" })]
    public async Task<IActionResult> RequestCodeAsync(RegisterModel model)
    {
        // Get the account with the preapproved email address.
        // If multiple accounts have the same email address we cannot preapprove.
        var users = _userService.FindByEmail(model.Email);
        if (users.Count() != 1) return new JsonResult(new RegisterModel(model.Email, "Your account is not preapproved"));

        var preapproved = users.First();
        if (preapproved == null) return new JsonResult(new RegisterModel(model.Email, "Your account is not preapproved"));
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
            user.RolesManyToMany.AddRange(preapproved.RolesManyToMany.Select(r => new UserRole(user.Id, r.RoleId)));
            await _keycloakHelper.UpdateUserAsync(user);
            _userService.Delete(preapproved);
            return new JsonResult(new RegisterModel(model.Email, user.Status, $"An email has been sent to {model.Email}"));
        }
        else
        {
            // TODO: Send email.
            var rnd = new Random();
            user.Code = $"{rnd.Next()}";
            _userService.Update(user);

            return new JsonResult(new RegisterModel(model.Email, UserStatus.Approved, "Your account has been approved"));
        }
    }

    /// <summary>
    /// Update user for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("request/approval")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "User" })]
    public IActionResult RequestApproval(Admin.Models.User.UserModel model)
    {
        // Only allow a user to update their own account.
        var username = this.User.GetUsername() ?? throw new NotAuthorizedException("Cannot update user");
        var original = _userService.FindByUsername(username);
        if (original == null) throw new InvalidOperationException("Cannot update user");

        original.Note = model.Note;
        original.Status = Entities.UserStatus.Requested;
        var result = _userService.Update(original);
        return new JsonResult(new Admin.Models.User.UserModel(result));
    }
    #endregion
}
