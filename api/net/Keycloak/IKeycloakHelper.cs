using System.Security.Claims;

namespace TNO.API.Keycloak;

/// <summary>
/// IKeycloakHelper interface, provides helper methods to manage and sync Keycloak.
/// </summary>
public interface IKeycloakHelper
{
    #region Methods
    /// <summary>
    /// Sync Keycloak 'Key' value in users, groups, and roles with the TNO users, roles, and claims.
    /// </summary>
    /// <returns></returns>
    Task SyncAsync();

    /// <summary>
    /// Activate the user with TNO and Keycloak.
    /// If the user doesn't currently exist in TNO, activate a new user by adding them to TNO.
    /// If the user exists in TNO, activate user by linking to Keycloak and updating Keycloak.
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    Task<Entities.User?> ActivateAsync(ClaimsPrincipal principal);

    /// <summary>
    /// Update the user in keycloak linked to this user.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<Entities.User?> UpdateUserAsync(Entities.User entity);

    /// <summary>
    /// Delete the user from keycloak linked to the specified 'entity'.
    /// If the user 'Key' is not linked it will do nothing.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task DeleteUserAsync(Entities.User entity);
    #endregion
}
