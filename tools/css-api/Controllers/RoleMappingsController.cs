using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.Core.Exceptions;
using TNO.CSS.API.Config;
using TNO.CSS.Models;
using TNO.Keycloak;

namespace TNO.CSS.API.Controllers;

/// <summary>
///
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/integrations/{integrationId}/{environment}/user-role-mappings")]
public class RoleMappingsController : ControllerBase
{
    #region Variables
    private readonly IKeycloakService _service;
    private readonly CssOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="options"></param>
    public RoleMappingsController(IKeycloakService service, IOptions<CssOptions> options)
    {
        _service = service;
        _options = options.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<RoleModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.UnprocessableEntity)]
    [SwaggerOperation(Tags = new[] { "RoleMappings" })]
    public async Task<IActionResult> GetUserRoleMappingsAsync(string? roleName, string? username)
    {
        var clientId = _options.ClientId ?? throw new ConfigurationException("Keycloak 'ClientId' is not configured.");

        if (!String.IsNullOrWhiteSpace(username))
        {
            var users = await _service.GetUsersAsync(0, 10, new TNO.Keycloak.UserFilter() { Username = username, Exact = true });
            if (users.Length == 0) return NotFound(new ErrorResponseModel());

            var user = users.First();
            var roles = await _service.GetUserClientRolesAsync(user.Id, clientId);

            return new JsonResult(new UserRoleResponseModel()
            {
                Users = new[] {
                  new UserModel() {
                    Username = user.Username ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Attributes = user.Attributes ?? new Dictionary<string, string[]>()
                  }
                },
                Roles = roles.Select(r => new RoleModel(r.Name ?? "", r.Composite)).ToArray()
            });
        }
        else if (!String.IsNullOrWhiteSpace(roleName))
        {
            var role = await _service.GetRoleAsync(clientId, roleName);
            if (role == null) return NotFound(new ErrorResponseModel());

            var users = await _service.GetRoleMembersAsync(clientId, roleName, 0, 100);

            return new JsonResult(new UserRoleResponseModel()
            {
                Users = users.Select(u => new UserModel()
                {
                    Username = u.Username ?? "",
                    Email = u.Email ?? "",
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    Attributes = u.Attributes ?? new Dictionary<string, string[]>()
                }).ToArray(),
                Roles = new[] {
                    new RoleModel(role.Name ?? "", role.Composite)
                }
            });
        }
        return BadRequest(new ErrorResponseModel());
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserRoleResponseModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.UnprocessableEntity)]
    [SwaggerOperation(Tags = new[] { "RoleMappings" })]
    public async Task<IActionResult> UpdateUserRoleMappingsAsync(UserRoleModel mapping, ApiVersion version)
    {
        var clientId = _options.ClientId ?? throw new ConfigurationException("Keycloak 'ClientId' is not configured.");

        var users = await _service.GetUsersAsync(0, 10, new Keycloak.UserFilter() { Username = mapping.Username });
        if (users.Length == 0) return NotFound(new ErrorResponseModel());
        else if (users.Length > 1) return BadRequest(new ErrorResponseModel());

        var user = users.First();
        var role = await _service.GetRoleAsync(clientId, mapping.RoleName);
        if (role == null) return NotFound(new ErrorResponseModel());

        if (mapping.Operation.Value == UserRoleOperation.Add.Value)
        {
            await _service.AddUserClientRolesAsync(user.Id, clientId, new[] { role });

            var roles = await _service.GetUserClientRolesAsync(user.Id, clientId);
            var model = new UserRoleResponseModel()
            {
                Users = new[] {
                  new UserModel() {
                    Username = user.Username ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Attributes = user.Attributes ?? new Dictionary<string, string[]>()
                  }
                },
                Roles = roles.Select(r => new RoleModel(r.Name ?? "", r.Composite)).ToArray()
            };
            return CreatedAtAction("GetUserRoleMappings", new
            {
                username = mapping.Username,
                version = version.ToString(),
                integrationId = this.RouteData.Values["integrationId"],
                environment = this.RouteData.Values["environment"]
            }, model);
        }
        else if (mapping.Operation.Value == UserRoleOperation.Delete.Value)
        {
            await _service.RemoveUserClientRolesAsync(user.Id, clientId, new[] { role });
            return NoContent();
        }

        return BadRequest(new ErrorResponseModel());
    }
    #endregion
}
