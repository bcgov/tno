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
    /// <summary>
    /// Determine if the current user has the specified role.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requirement"></param>
    /// <returns></returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, KeycloakClientRoleRequirement requirement)
    {
        var claim = context.User.Claims.FirstOrDefault(c => c.Type == "resource_access");
        if (claim == null)
        {
            context.Fail();
        }
        else
        {
            var resourceAccess = JsonSerializer.Deserialize<ResourceAccessModel>(claim.Value);
            if (resourceAccess?.App.Roles.Intersect(requirement.Role.Select(r => r.GetName())).Any() ?? false)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        return Task.CompletedTask;
    }
}
