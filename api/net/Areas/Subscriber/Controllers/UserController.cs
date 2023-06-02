using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.CSS;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.API.Areas.Subscriber.Models;
using TNO.Keycloak;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;

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
    private readonly ICssHelper _cssHelper;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserController object, initializes with specified parameters.
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="cssHelper"></param>
    public UserController(IUserService userService, ICssHelper cssHelper, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _userService = userService;
        _cssHelper = cssHelper;
        _serializerOptions = serializerOptions.Value;


    }
    #endregion

    #region Endpoints
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
    public IActionResult Update(UserModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

        if (user.Id != model.Id)
        {
            throw new NotAuthorizedException("You are not authorized to update this user.");
        }
        var result = _userService.UpdatePreferences((User)model);
        return new JsonResult(new UserModel(result!, _serializerOptions));
    }
    #endregion
}
