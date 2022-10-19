using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using static TNO.Keycloak.ClientRoleAuthorizeAttribute;

namespace TNO.Keycloak;

/// <summary>
/// ClientRoleAuthorizationPolicyProvider class,
/// </summary>
public class ClientRoleAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ClientRoleAuthorizationPolicyProvider object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public ClientRoleAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }
    #endregion

    #region Methods
    /// <summary>
    /// Extract the policies from the policy name.
    /// </summary>
    /// <param name="policyName"></param>
    /// <returns></returns>
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            return await base.GetPolicyAsync(policyName);

        // Will extract the permissions from the string (Create, Update..)
        var roles = GetRolesFromPolicy(policyName);

        // Here we create the instance of our requirement
        var requirement = new KeycloakClientRoleRequirement(roles);

        // Now we use the builder to create a policy, adding our requirement
        return new AuthorizationPolicyBuilder()
            .AddRequirements(requirement).Build();
    }
    #endregion
}
