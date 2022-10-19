using System;
using Microsoft.AspNetCore.Authorization;

namespace TNO.Keycloak;

/// <summary>
/// KeycloakClientRoleRequirement class, provides a way to ensure a user has the specified role.
/// </summary>
public class KeycloakClientRoleRequirement : IAuthorizationRequirement
{
    #region Properties
    /// <summary>
    /// get - The role to validate.
    /// </summary>
    public ClientRole[] Role { get; private set; } = Array.Empty<ClientRole>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KeycloakClientRoleRequirement class.
    /// </summary>
    /// <param name="role"></param>
    public KeycloakClientRoleRequirement(ClientRole role)
    {
        this.Role = new[] { role };
    }

    /// <summary>
    /// Creates a new instance of a KeycloakClientRoleRequirement class.
    /// </summary>
    /// <param name="role"></param>
    public KeycloakClientRoleRequirement(params ClientRole[] role)
    {
        this.Role = role;
    }
    #endregion
}
