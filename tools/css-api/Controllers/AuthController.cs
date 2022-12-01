using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.Core.Extensions;
using TNO.Core.Http.Models;
using TNO.CSS.API.Models;
using TNO.CSS.Models;
using TNO.Keycloak;

namespace TNO.CSS.API.Controllers;

/// <summary>
///
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}")]
public class AuthController : ControllerBase
{
    #region Variables
    private readonly IKeycloakService _client;
    #endregion

    #region Constructors
    /// <summary>
    ///
    /// </summary>
    /// <param name="client"></param>
    public AuthController(IKeycloakService client)
    {
        _client = client;
    }
    #endregion

    #region Endpoints
    /// <summary>
    ///
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    [HttpPost("token")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TokenModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "auth" })]
    public async Task<IActionResult> GetAccessTokenAsync([FromForm] RequestTokenFormModel form)
    {
        if (form.GrantType != "client_credentials") return BadRequest(new ErrorResponseModel());

        var token = await _client.RequestTokenAsync();
        return new JsonResult(token);
    }
    #endregion
}
