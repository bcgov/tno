using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace TNO.Keycloak;

/// <summary>
/// ClientRoleAuthorizeAttribute class, provides a way to control authorization for controllers and endpoints.
/// Use one or more role names that have access to the endpoints.
/// </summary>
public class ClientRoleAuthorizeAttribute : AuthorizeAttribute
{
    #region Variables
    internal const string PolicyPrefix = "CLIENT_ROLES_";
    private const string Separator = "_";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ClientRoleAuthorizeAttribute object, initializes with specified parameters.
    /// </summary>
    /// <param name="role"></param>
    public ClientRoleAuthorizeAttribute(params ClientRole[] role)
    {
        this.Policy = $"{PolicyPrefix}{String.Join(Separator, role)}";
    }
    #endregion

    #region Methods
    /// <summary>
    /// Extract the roles names from the policy name.
    /// </summary>
    /// <param name="policyName"></param>
    /// <returns></returns>
    public static ClientRole[] GetRolesFromPolicy(string policyName)
    {
        return policyName[PolicyPrefix.Length..]
            .Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries)
            .Select(r => Enum.Parse<ClientRole>(r))
            .ToArray();
    }
    #endregion
}
