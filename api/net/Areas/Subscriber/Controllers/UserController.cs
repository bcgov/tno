using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.User;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// UserController class, provides User endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/users")]
[Route("api/[area]/users")]
[Route("v{version:apiVersion}/[area]/users")]
[Route("[area]/users")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class UserController : ControllerBase
{
    #region Variables
    private readonly IUserService _userService;
    private readonly IUserColleagueService _userColleagueService;
    private readonly IImpersonationHelper _impersonate;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserController object, initializes with specified parameters.
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="impersonateHelper"></param>
    /// <param name="userColleagueService"></param>
    /// <param name="serializerOptions"></param>
    public UserController(
        IUserService userService,
        IImpersonationHelper impersonateHelper,
        IUserColleagueService userColleagueService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _userService = userService;
        _impersonate = impersonateHelper;
        _userColleagueService = userColleagueService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find the current user, or the impersonated user.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "User" })]
    public IActionResult FindById()
    {
        var user = _impersonate.GetCurrentUser();
        return new JsonResult(new UserModel(user));
    }

    /// <summary>
    /// Update user for the specified 'id'.
    /// Update the user in Keycloak if the 'Key' is linked.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "User" })]
    public IActionResult Update([FromBody] UserModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var isAdmin = user.Roles.Split(',').Contains($"[{ClientRole.Administrator.GetName()}]");
        if (user.Id != model.Id && !isAdmin) throw new NotAuthorizedException("You are not authorized to update this user.");
        var result = _userService.UpdatePreferences((User)model) ?? throw new NoContentException("Updated did not return the user");
        return new JsonResult(new UserModel(result));
    }

    /// <summary>
    /// Find colleagues related to logged user.
    /// </summary>
    /// <returns></returns>
    [HttpGet("colleagues")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserColleagueModel[]), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Colleague" })]
    public IActionResult FindColleaguesByUserId()
    {
        var user = _impersonate.GetCurrentUser();
        var colleagues = _userColleagueService.FindColleaguesByUserId(user.Id).Select(m => new UserColleagueModel(m));
        return new JsonResult(colleagues);
    }

    /// <summary>
    /// Add the colleague linked to the specified 'email' to the current user.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("colleagues")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserColleagueModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Colleague" })]
    public IActionResult AddColleague(string email)
    {
        if (String.IsNullOrWhiteSpace(email)) throw new ArgumentException("Parameter 'email' is required.");

        var user = _impersonate.GetCurrentUser();
        var colleague = _userService.FindByEmail(email).FirstOrDefault() ?? throw new InvalidOperationException("There is no user with this email.");
        var result = _userColleagueService.AddColleague(new UserColleague(user.Id, colleague.Id));
        return CreatedAtAction(nameof(AddColleague), new { id = result.ColleagueId }, new UserColleagueModel(result));
    }

    /// <summary>
    /// Add user for the specified 'id'.
    /// </summary>
    /// <param name="colleagueId"></param>
    /// <returns></returns>
    [HttpDelete("colleagues/{colleagueId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserColleagueModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Colleague" })]
    public IActionResult DeleteColleague(int colleagueId)
    {
        var user = _impersonate.GetCurrentUser();
        var result = _userColleagueService.RemoveColleague(user.Id, colleagueId) ?? throw new InvalidOperationException("No colleague to delete.");
        var deletedModel = new UserColleagueModel(result, _serializerOptions);
        return CreatedAtAction(nameof(DeleteColleague), new { id = result?.ColleagueId }, deletedModel);
    }
    #endregion
}
