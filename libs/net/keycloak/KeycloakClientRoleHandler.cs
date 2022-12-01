using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using TNO.Keycloak.Models;
using TNO.Core.Extensions;
using System.Threading.Tasks;
using System.Linq;

namespace TNO.Keycloak;

/// <summary>
/// KeycloakClientRoleHandler class, provides a way to validate whether the user has a specified role.
/// </summary>
public class KeycloakClientRoleHandler : AuthorizationHandler<KeycloakClientRoleRequirement>
{
    #region Variables
    private const string RESOURCE_ACCESS = "resource_access";
    private const string CLIENT_ROLES = "client_roles";
    #endregion

    #region Methods
    /// <summary>
    /// Determine if the current user has the specified role.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requirement"></param>
    /// <returns></returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, KeycloakClientRoleRequirement requirement)
    {
        var claims = context.User.Claims.Where(c => c.Type == RESOURCE_ACCESS || c.Type == CLIENT_ROLES);
        if (!claims.Any())
        {
            context.Fail();
        }
        else
        {
            var standard = claims.FirstOrDefault(c => c.Type == RESOURCE_ACCESS);
            if (standard != null)
            {
                var resourceAccess = JsonSerializer.Deserialize<ResourceAccessModel>(standard.Value);
                if (resourceAccess?.App.Roles.Intersect(requirement.Role.Select(r => r.GetName())).Any() ?? false)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else
            {
                if (claims.Select(c => c.Value).Intersect(requirement.Role.Select(r => r.GetName())).Any())
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
        }

        return Task.CompletedTask;
    }
    #endregion
}
