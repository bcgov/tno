using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.User;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// UserController class, provides User endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
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
    private readonly IUserService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public UserController(IUserService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find the user for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "User" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);
        if (result == null) return NoContent();
        return new JsonResult(new UserModel(result));
    }

    /// <summary>
    /// Find all the users in the distribution list for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/distribution")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<UserModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "User" })]
    public IActionResult FindDistributionListById(int id)
    {
        var result = _service.GetDistributionList(id);
        return new JsonResult(result.Select(r => new UserModel(r)));
    }
    #endregion
}
